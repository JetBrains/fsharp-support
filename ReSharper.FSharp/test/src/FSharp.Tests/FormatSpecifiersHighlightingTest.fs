namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon

open JetBrains.ReSharper.Daemon.Impl
open JetBrains.ReSharper.FeaturesTestFramework.Daemon
open JetBrains.ReSharper.Plugins.FSharp.ProjectModel.ProjectProperties
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Tests
open NUnit.Framework

[<FSharpTest>]
type FormatSpecifiersHighlightingTest() =
    inherit HighlightingTestBase()

    override x.RelativeTestDataPath = "features/daemon/formatSpecifiersHighlighting"

    override x.CompilerIdsLanguage = FSharpLanguage.Instance :> _

    override x.GetProjectProperties(targetFrameworkIds, _) =
        FSharpProjectPropertiesFactory.CreateProjectProperties(targetFrameworkIds)

    override x.HighlightingPredicate(highlighting, _, _) = highlighting :? FormatStringItemHighlighting

    [<Test>] member x.``Bindings``() = x.DoNamedTest()
    [<Test>] member x.``Try with finally``() = x.DoNamedTest()
    [<Test>] member x.``Record and union members``() = x.DoNamedTest()
    [<Test>] member x.``Escaped strings``() = x.DoNamedTest()
    [<Test>] member x.``Triple quoted strings``() = x.DoNamedTest()
    [<Test>] member x.``Multi line strings``() = x.DoNamedTest()
    [<Test>] member x.``Malformed formatters``() = x.DoNamedTest()
    [<Test>] member x.``kprintf bprintf fprintf``() = x.DoNamedTest()
    [<Test>] member x.``Plane strings``() = x.DoNamedTest()
    [<Test>] member x.``Extensions``() = x.DoNamedTest()
