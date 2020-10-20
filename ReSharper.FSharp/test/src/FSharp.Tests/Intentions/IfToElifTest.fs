namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Intentions
open JetBrains.ReSharper.Plugins.FSharp.Tests
open JetBrains.ReSharper.TestFramework
open NUnit.Framework

[<FSharpTest; TestPackages(FSharpCorePackage)>]
type IfToElifTest() =
    inherit FSharpContextActionExecuteTestBase<IfToElifAction>()

    override x.ExtraPath = "ifToElif"

    [<Test>] member x.``Simple 01``() = x.DoNamedTest()
    [<Test>] member x.``Simple 02 - No spaces``() = x.DoNamedTest()
