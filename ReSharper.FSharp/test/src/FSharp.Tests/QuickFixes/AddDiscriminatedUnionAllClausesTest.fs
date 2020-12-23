namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.FeaturesTestFramework.Intentions
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Tests
open JetBrains.ReSharper.TestFramework
open NUnit.Framework

[<FSharpTest>]
type AddDiscriminatedUnionAllClausesAvailabilityTest() =
    inherit QuickFixAvailabilityTestBase()

    override x.RelativeTestDataPath = "features/quickFixes/addDiscriminatedUnionAllClauses"

    [<Test>] member x.``Union literal availability``() = x.DoNamedTest()
    [<Test>] member x.``Non-union variable``() = x.DoNamedTest()
    [<Test>] member x.``WildPat completion doesn't require quickfix``() = x.DoNamedTest()

[<FSharpTest; TestPackages("FSharp.Core")>]
type AddDiscriminatedUnionAllClausesTest() =
    inherit QuickFixTestBase<AddDiscriminatedUnionAllClauses>()

    override x.RelativeTestDataPath = "features/quickFixes/addDiscriminatedUnionAllClauses"

    [<Test>] member x.``Simple 01``() = x.DoNamedTest()
    [<Test>] member x.``Simple in context 01``() = x.DoNamedTest()
    [<Test>] member x.``Simple single existing case 01``() = x.DoNamedTest()
    
    [<Test>] member x.``When statements make match incomplete``() = x.DoNamedTest()
    [<Test>] member x.``Nested union match incomplete with literals``() = x.DoNamedTest()
    [<Test>] member x.``Complete match with literals generates erroneous statement``() = x.DoNamedTest()
    [<Test>] member x.``Complete match with whens generates erroneous statement``() = x.DoNamedTest()
    
    [<Test>] member x.``Unconditional with when statements not repeated``() = x.DoNamedTest()
    [<Test>] member x.``Catchall partial parameter``() = x.DoNamedTest()
    