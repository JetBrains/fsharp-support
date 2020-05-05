namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon

open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Tests.Common
open JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon
open NUnit.Framework

[<ExpectErrors 39>]
type RedundantAttributeParensTest() =
    inherit FSharpHighlightingTestBase()

    override x.RelativeTestDataPath = "features/daemon/redundantAttributeParens"

    override x.HighlightingPredicate(highlighting, _, _) =
        highlighting :? RedundantAttributeParensWarning
        
    [<Test>] member x.``Type 01 - No arguments``() = x.DoNamedTest()
    [<Test>] member x.``Type 02 - Has arguments``() = x.DoNamedTest()
    [<Test>] member x.``Type 03 - No parens``() = x.DoNamedTest()
    [<Test>] member x.``Type 04 - Multiple attributes``() = x.DoNamedTest()
    [<Test>] member x.``Type 05 - Target attribute``() = x.DoNamedTest()
