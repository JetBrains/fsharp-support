namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon

open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon
open NUnit.Framework

[<Explicit>]
type RedundantParensTest() =
    inherit FSharpHighlightingTestBase()

    override x.RelativeTestDataPath = "features/daemon/redundantParens"

    override x.HighlightingPredicate(highlighting, _, _) =
        highlighting :? RedundantParenExprWarning
    
    [<Test>] member x.``Literals 01``() = x.DoNamedTest()

    [<Test>] member x.``App - Local 01``() = x.DoNamedTest()
    [<Test>] member x.``App - Top level 01``() = x.DoNamedTest()

    [<Test>] member x.``App - Precedence - High 01``() = x.DoNamedTest()
    [<Test>] member x.``App - Precedence - High 02 - Multiple``() = x.DoNamedTest()
    [<Test>] member x.``App - Precedence - High 03 - Multiple - Last``() = x.DoNamedTest()

    [<Test>] member x.``App - Precedence - Low 01``() = x.DoNamedTest()

    [<Test>] member x.``App - Precedence - List 01``() = x.DoNamedTest()
    [<Test>] member x.``App - Precedence - List 02``() = x.DoNamedTest()
    [<Test>] member x.``App - Precedence - List 03``() = x.DoNamedTest()

    [<Test>] member x.``App - Precedence - Indexer 01``() = x.DoNamedTest()
    [<Test>] member x.``App - Precedence - Indexer 02 - Multiple``() = x.DoNamedTest()

    [<Test>] member x.``Arg - High precedence 01``() = x.DoNamedTest()
    [<Test>] member x.``Arg - High precedence 02 - Member``() = x.DoNamedTest()

    [<Test>] member x.``Arg - Low precedence 01``() = x.DoNamedTest()
    [<Test>] member x.``Arg - Low precedence 02 - Member``() = x.DoNamedTest()
