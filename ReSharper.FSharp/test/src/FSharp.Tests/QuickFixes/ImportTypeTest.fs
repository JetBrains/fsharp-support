namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features

open JetBrains.ReSharper.FeaturesTestFramework.Intentions
open JetBrains.ReSharper.Intentions.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Tests
open JetBrains.ReSharper.TestFramework
open NUnit.Framework

[<FSharpTest; TestPackages("FSharp.Core")>]
type ImportTypeTest() =
    inherit QuickFixTestBase<ImportTypeFix>()

    override x.RelativeTestDataPath = "features/quickFixes/importType"

    override x.OnQuickFixNotAvailable(_, _) = Assert.Fail(ErrorText.NotAvailable);

    [<Test>] member x.``Type 01``() = x.DoNamedTest()
    [<Test>] member x.``Type extension 01``() = x.DoNamedTest()

    [<Test>] member x.``Generic List 01``() = x.DoNamedTest()
    [<Test>] member x.``Generic List 02``() = x.DoNamedTest()
    [<Test>] member x.``Generic List 03``() = x.DoNamedTest()
    [<Test>] member x.``Generic List 04``() = x.DoNamedTest()

    [<Test>] member x.``Attribute 01``() = x.DoNamedTest()

    [<Test>] member x.``Type arguments - Count 01``() = x.DoNamedTest()
    [<Test>] member x.``Module name - Escaped 01``() = x.DoNamedTest()

    [<Test; NotAvailable>] member x.``Not available 01 - Open``() = x.DoNamedTest()
