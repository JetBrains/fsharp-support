[<AutoOpen>]
module JetBrains.ReSharper.Plugins.FSharp.Tests.Common

open System
open System.Threading
open FSharp.Compiler.SourceCodeServices
open JetBrains.Application.Components
open JetBrains.Application.platforms
open JetBrains.DataFlow
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ProjectModel.MSBuild
open JetBrains.ProjectModel.Properties.Managed
open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.ReSharper.Plugins.FSharp.Checker
open JetBrains.ReSharper.Plugins.FSharp.ProjectModel.ProjectItems.ItemsContainer
open JetBrains.ReSharper.Plugins.FSharp.ProjectModel.ProjectProperties
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.TestFramework
open JetBrains.TestFramework.Projects
open JetBrains.Util.Dotnet.TargetFrameworkIds
open NUnit.Framework
open Moq

[<assembly: Apartment(ApartmentState.STA)>]
do()

type FSharpTestAttribute() =
    inherit TestProjectFilePropertiesProvider(FSharpProjectFileType.FsExtension, MSBuildProjectUtil.CompileElement)

    let targetFrameworkId =
        TargetFrameworkId.Create(FrameworkIdentifier.NetFramework, new Version(4, 5, 1), ProfileIdentifier.Default)

    interface ITestPlatformProvider with
        member x.GetTargetFrameworkId() = targetFrameworkId

    interface ITestFileExtensionProvider with
        member x.Extension = FSharpProjectFileType.FsExtension

    interface ITestProjectPropertiesProvider with
        member x.GetProjectProperties(targetFrameworkIds, _) =
            FSharpProjectPropertiesFactory.CreateProjectProperties(targetFrameworkIds)


let itemsContainer = Mock<IFSharpItemsContainer>().Object

[<SolutionComponent>]
type FSharpTestProjectOptionsBuilder(checkerService, psiModules, logger, resolveContextManager) =
    inherit FSharpProjectOptionsBuilder(checkerService, psiModules, logger, resolveContextManager, itemsContainer)

    override x.GetProjectItemsPaths(_, _) = [||]

    interface IHideImplementation<IFSharpProjectOptionsBuilder>


[<SolutionComponent>]
type FSharpTestProjectOptionsProvider
        (lifetime: Lifetime, checkerService: FSharpCheckerService, projectOptionsBuilder: IFSharpProjectOptionsBuilder,
         scriptOptionsProvider: FSharpScriptOptionsProvider) as this =
    do
        checkerService.OptionsProvider <- this
        lifetime.OnTermination(fun _ -> checkerService.OptionsProvider <- Unchecked.defaultof<_>) |> ignore

    let getProjectOptions (sourceFile: IPsiSourceFile) =
        let fsProject = projectOptionsBuilder.BuildSingleFSharpProject(sourceFile.GetProject(), sourceFile.PsiModule)
        Some { fsProject.ProjectOptions with SourceFiles = [| sourceFile.GetLocation().FullPath |] }

    interface IHideImplementation<FSharpProjectOptionsProvider>
    
    interface IFSharpProjectOptionsProvider with
        member x.HasPairFile _ = false

        member x.GetProjectOptions(sourceFile) =
            if sourceFile.LanguageType.Is<FSharpScriptProjectFileType>() then
                scriptOptionsProvider.GetScriptOptions(sourceFile) else

            getProjectOptions sourceFile

        member x.GetParsingOptions(sourceFile) =
            let isScript = sourceFile.LanguageType.Is<FSharpScriptProjectFileType>()

            let isExe =
                match sourceFile.GetProject() with
                | null -> false
                | project ->

                let targetFrameworkId = sourceFile.PsiModule.TargetFrameworkId
                match project.ProjectProperties.ActiveConfigurations.GetOrCreateConfiguration(targetFrameworkId) with
                | :? IManagedProjectConfiguration as cfg ->
                    cfg.OutputType = ProjectOutputType.CONSOLE_EXE
                | _ -> false

            { FSharpParsingOptions.Default with
                SourceFiles = [| sourceFile.GetLocation().FullPath |]
                IsExe = isExe
                IsInteractive = isScript }

        member x.GetFileIndex(_) = 0
        member x.ModuleInvalidated = new Signal<_>("Todo") :> _
