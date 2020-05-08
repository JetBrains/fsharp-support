module JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Analyzers.ColorReferenceAnalyzer

open JetBrains.Application.Settings
open JetBrains.ReSharper.Daemon.VisualElements
open JetBrains.ReSharper.Feature.Services.Daemon
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Stages
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree

[<ElementProblemAnalyzer(typeof<IReferenceExpr>,
                         HighlightingTypes = [| typeof<ColorHighlighting> |])>]
type RedundantAttributeSuffixAnalyzer() =
    inherit ElementProblemAnalyzer<IReferenceExpr>()

    override x.Run(expr, analyzerData, consumer) =
        let visualElementHighlighter = analyzerData.GetData(FSharpErrorsStage.visualElementFactoryKey)
        let info = visualElementHighlighter.CreateColorHighlightingInfo(expr)
        if isNotNull info then
            consumer.AddHighlighting(info.Highlighting, info.Range)

    interface IConditionalElementProblemAnalyzer with
        member x.ShouldRun(_, analyzerData) =
            analyzerData.SettingsStore.GetValue(HighlightingSettingsAccessor.ColorUsageHighlightingEnabled)
