namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon

open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.ReSharper.TestFramework
open NUnit.Framework

type ErrorsHighlightingTest() =
    inherit FSharpHighlightingTestBase()

    override x.RelativeTestDataPath = "features/daemon/errorsHighlighting"

    [<Test>] member x.``Empty file``() = x.DoNamedTest()
    [<Test>] member x.``No errors 01``() = x.DoNamedTest()
    [<Test>] member x.``Multiline range``() = x.DoNamedTest()

    [<Test>] member x.``Syntax errors 01``() = x.DoNamedTest()
    [<Test>] member x.``Syntax errors 02``() = x.DoNamedTest()

    [<Test>] member x.``Type check errors 01 - type mismatch``() = x.DoNamedTest()
    [<Test>] member x.``Type check errors 02 - nested error``() = x.DoNamedTest()

    [<Test>] member x.``Use not allowed 01 - module``() = x.DoNamedTest()
    [<Test>] member x.``Use not allowed 02 - primary ctor``() = x.DoNamedTest()

    [<Test>] member x.``Error - no inherit lid``() = x.DoNamedTest()

    [<TestFileExtension(FSharpScriptProjectFileType.FsxExtension)>]
    [<Test>] member x.``Unused value in script``() = x.DoNamedTest()

    [<Test>] member x.``Unfinished let``() = x.DoNamedTest()

    [<Test>] member x.``Rule never matched 01``() = x.DoNamedTest()
    [<Test>] member x.``Rule never matched 02 - Function``() = x.DoNamedTest()
