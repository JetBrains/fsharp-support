﻿namespace JetBrains.ReSharper.Plugins.FSharp.Tests.Features.Daemon

open JetBrains.ReSharper.Plugins.FSharp.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp.Tests
open JetBrains.ReSharper.TestFramework
open NUnit.Framework

[<FSharpTest; TestPackages(FSharpCorePackage)>]
type ErrorsHighlightingTest() =
    inherit FSharpHighlightingTestBase()

    override x.RelativeTestDataPath = "features/daemon/errorsHighlighting"

    [<Test>] member x.``Enum Rqa analyzer 01``() = x.DoNamedTest()

    [<Test>] member x.``Extension analyzer``() = x.DoNamedTest()

    [<Test>] member x.``ListConsPat analyzer - Empty list tail 01``() = x.DoNamedTest()

    [<Test>] member x.``Redundant union case pattern - Active pattern 01``() = x.DoNamedTest()
    [<Test>] member x.``Redundant union case pattern - Nested 01``() = x.DoNamedTest()

    [<Test>] member x.``Self id 01``() = x.DoNamedTest()
    [<Test>] member x.``Self id 02 - Property with accessors``() = x.DoNamedTest()

    [<FSharpLanguageLevel(FSharpLanguageLevel.FSharp46)>]
    [<Test>] member x.``Self id - Not available 01``() = x.DoNamedTest()
