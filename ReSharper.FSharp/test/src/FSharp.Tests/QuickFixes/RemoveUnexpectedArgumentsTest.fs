﻿namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.FeaturesTestFramework.Intentions
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Tests.Common
open NUnit.Framework

[<FSharpTest>]
[<ExpectErrors 3>]
type RemoveUnexpectedArgumentsTest() =
    inherit QuickFixTestBase<RemoveUnexpectedArgumentsFix>()
    
    override x.RelativeTestDataPath = "features/quickFixes/removeUnexpectedArguments"
    
    [<Test>] member x.``Simple - with unit args``() = x.DoNamedTest()
    [<Test>] member x.``Function 1 - one arg``() = x.DoNamedTest()
    [<Test>] member x.``Function 2 - in expression``() = x.DoNamedTest()    
    [<Test>] member x.``Function 3 - many args``() = x.DoNamedTest()
    [<Test>] member x.``Function 4 - expression args``() = x.DoNamedTest()
    [<Test>] member x.``Function 5 - several errors in single line 1``() = x.DoNamedTest()
    [<Test>] member x.``Function 5 - several errors in single line 2``() = x.DoNamedTest()
    [<Test>] member x.``Function 5 - several errors in single line 3``() = x.DoNamedTest()
    [<Test>] member x.``Function 6 - with single comment``() = x.DoNamedTest()
    [<Test>] member x.``Function 7 - with several comments``() = x.DoNamedTest()
    [<Test>] member x.``Function 8 - with multiline comment``() = x.DoNamedTest()
    [<Test>] member x.``Function 9 - with block comment``() = x.DoNamedTest()
    [<Test>] member x.``Property 1 - simple``() = x.DoNamedTest()
