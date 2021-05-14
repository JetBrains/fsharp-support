namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Tests
open NUnit.Framework

[<FSharpTest>]
type RemoveRedundantBackticksTest() =
    inherit FSharpQuickFixTestBase<RemoveRedundantBackticksFix>()

    override x.RelativeTestDataPath = "features/quickFixes/removeRedundantBackticks"

    [<Test>] member x.``Simple 01``() = x.DoNamedTest()
    [<Test>] member x.``Simple 02 - insert whitespaces``() = x.DoNamedTest()

    [<Test; NoHighlightingFound>] member x.``Keyword 01``() = x.DoNamedTest()
