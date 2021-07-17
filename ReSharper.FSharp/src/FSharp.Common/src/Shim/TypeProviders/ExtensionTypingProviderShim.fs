namespace JetBrains.ReSharper.Plugins.FSharp.Shim.TypeProviders

open System
open FSharp.Compiler
open FSharp.Compiler.ExtensionTyping
open FSharp.Compiler.Text
open FSharp.Core.CompilerServices
open JetBrains.Core
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp.Checker
open JetBrains.ReSharper.Plugins.FSharp.Settings
open JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Protocol
open JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Protocol.Exceptions
open JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Protocol.Models

type IProxyExtensionTypingProvider =
    inherit IExtensionTypingProvider

    abstract RuntimeVersion: unit -> string
    abstract DumpTypeProvidersProcess: unit -> string

[<SolutionComponent>]
type ExtensionTypingProviderShim(solution: ISolution, toolset: ISolutionToolset,
        experimentalFeatures: FSharpExperimentalFeaturesProvider, fcsProjectProvider: FcsProjectProvider,
        typeProvidersLoadersFactory: TypeProvidersExternalProcessFactory) as this =
    let lifetime = solution.GetLifetime()
    let defaultShim = ExtensionTypingProvider
    let outOfProcess = experimentalFeatures.OutOfProcessTypeProviders
    let createProcessLockObj = obj()

    let [<VolatileField>] mutable connection: TypeProvidersConnection = null
    let mutable typeProvidersHostLifetime: LifetimeDefinition = null
    let mutable typeProvidersManager = Unchecked.defaultof<IProxyTypeProvidersManager>

    let isConnectionAlive () =
        isNotNull connection && connection.IsActive

    let terminateConnection () =
        if isConnectionAlive() then typeProvidersHostLifetime.Terminate()

    let connect () =
        if isConnectionAlive () then () else

        lock createProcessLockObj (fun () ->
            if isConnectionAlive () then () else

            typeProvidersHostLifetime <- Lifetime.Define(lifetime)
            let newConnection = typeProvidersLoadersFactory.Create(typeProvidersHostLifetime.Lifetime).Run()
            typeProvidersManager <- TypeProvidersManager(newConnection) :?> _
            connection <- newConnection)

    do
        lifetime.Bracket((fun () -> ExtensionTypingProvider <- this),
            fun () -> ExtensionTypingProvider <- defaultShim)

        toolset.Changed.Advise(lifetime, fun _ -> terminateConnection ())
        outOfProcess.Change.Advise(lifetime, fun enabled ->
            if enabled.HasNew && not enabled.New then terminateConnection ())

        fcsProjectProvider.FcsProjectInvalidated.Advise(lifetime, fun psiModule ->
            if isNotNull typeProvidersManager then
                let project = psiModule.ContainingProjectModule.As<ProjectImpl>()
                for projectInfo in project.OutputAssemblies.GetAllInfo() do
                    typeProvidersManager.DisposeTypeProviders(projectInfo.OutputAssemblyInfo.Location.FullPath))

    interface IProxyExtensionTypingProvider with
        member this.InstantiateTypeProvidersOfAssembly(runTimeAssemblyFileName: string,
                designTimeAssemblyNameString: string, resolutionEnvironment: ResolutionEnvironment,
                isInvalidationSupported: bool, isInteractive: bool, systemRuntimeContainsType: string -> bool,
                systemRuntimeAssemblyVersion: Version, compilerToolsPath: string list,
                logError: TypeProviderError -> unit, m: range) =
            if not outOfProcess.Value then
               defaultShim.InstantiateTypeProvidersOfAssembly(runTimeAssemblyFileName, designTimeAssemblyNameString,
                    resolutionEnvironment, isInvalidationSupported, isInteractive,
                    systemRuntimeContainsType, systemRuntimeAssemblyVersion, compilerToolsPath, logError, m)
            else
                connect()
                try
                    typeProvidersManager.GetOrCreate(runTimeAssemblyFileName, designTimeAssemblyNameString,
                        resolutionEnvironment, isInvalidationSupported, isInteractive, systemRuntimeContainsType,
                        systemRuntimeAssemblyVersion, compilerToolsPath)
                with :? TypeProvidersInstantiationException as e  ->
                    logError (TypeProviderError(e.FcsNumber, "", m, [e.Message]))
                    []

        member this.GetProvidedTypes(pn: IProvidedNamespace) =
            match pn with
            | :? IProxyProvidedNamespace as pn -> pn.GetProvidedTypes()
            | _ -> defaultShim.GetProvidedTypes(pn)

        member this.ResolveTypeName(pn: IProvidedNamespace, typeName: string) =
            match pn with
            | :? IProxyProvidedNamespace as pn -> pn.ResolveProvidedTypeName typeName
            | _ -> defaultShim.ResolveTypeName(pn, typeName)

        member this.GetInvokerExpression(provider: ITypeProvider, method: ProvidedMethodBase, args: ProvidedVar []) =
            match provider with
            | :? IProxyTypeProvider as tp -> tp.GetInvokerExpression(method, args)
            | _ -> defaultShim.GetInvokerExpression(provider, method, args)

        member this.DisplayNameOfTypeProvider(provider: ITypeProvider, fullName: bool) =
            match provider with
            | :? IProxyTypeProvider as tp -> tp.GetDisplayName fullName
            | _ -> defaultShim.DisplayNameOfTypeProvider(provider, fullName)

        member this.RuntimeVersion() =
            if not (isConnectionAlive ()) then null else

            connection.Execute(fun _ -> connection.ProtocolModel.RdTestHost.RuntimeVersion.Sync(Unit.Instance))

        member this.DumpTypeProvidersProcess() =
            if not (isConnectionAlive ()) then raise (InvalidOperationException("Out-of-process disabled")) else

            let inProcessDump =
                $"[In-Process dump]:\n\n{typeProvidersManager.Dump()}"

            let outOfProcessDump =
                $"[Out-Process dump]:\n\n{connection.Execute(fun _ ->
                    connection.ProtocolModel.RdTestHost.Dump.Sync(Unit.Instance))}"

            $"{inProcessDump}\n\n{outOfProcessDump}"

    interface IDisposable with
        member this.Dispose() = terminateConnection ()
