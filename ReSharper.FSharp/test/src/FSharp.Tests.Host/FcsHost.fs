namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Host

open System.Linq
open JetBrains.Core
open JetBrains.Diagnostics
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ReSharper.Host.Features
open JetBrains.ReSharper.Plugins.FSharp.Common.Checker
open JetBrains.ReSharper.Plugins.FSharp.Common.Shim.FileSystem
open JetBrains.ReSharper.Plugins.FSharp.ProjectModel.ProjectItems.ItemsContainer
open JetBrains.Rider.Model
open Microsoft.FSharp.Compiler.AbstractIL.Internal.Library

[<SolutionComponent>]
type FcsHost
        (lifetime: Lifetime, solution: ISolution, checkerService: FSharpCheckerService,
         sourceCache: FSharpSourceCache, itemsContainer: FSharpItemsContainer) =

    let dumpSingleProjectMapping (rdVoid: Unit) =
        let projectMapping =
            itemsContainer.ProjectMappings.Values.SingleOrDefault().NotNull("Expected single project mapping.")
        projectMapping.DumpToString()

    do
        let fcsHost = solution.GetProtocolSolution().GetRdFSharpModel().FSharpCompilerServiceHost

        // We want to get events published by background checker.
        checkerService.Checker.ImplicitlyStartBackgroundWork <- true
        
        let subscription = checkerService.Checker.ProjectChecked.Subscribe(fun (projectFilePath, _) ->
            fcsHost.ProjectChecked(projectFilePath))
        lifetime.OnTermination(fun _ -> subscription.Dispose()) |> ignore

        fcsHost.GetLastModificationStamp.Set(Shim.FileSystem.GetLastWriteTimeShim)
        fcsHost.GetSourceCache.Set(sourceCache.GetRdFSharpSource)
        fcsHost.DumpSingleProjectMapping.Set(dumpSingleProjectMapping)
