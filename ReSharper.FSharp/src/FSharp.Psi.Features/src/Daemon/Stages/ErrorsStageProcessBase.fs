namespace JetBrains.ReSharper.Plugins.FSharp.Daemon.Stages

open System
open System.Collections.Generic
open JetBrains.DocumentModel
open JetBrains.ReSharper.Feature.Services.Daemon
open JetBrains.ReSharper.Plugins.FSharp.Common.Util
open JetBrains.ReSharper.Plugins.FSharp.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Daemon.Cs.Stages
open JetBrains.ReSharper.Plugins.FSharp.Psi.Util
open JetBrains.Util
open Microsoft.FSharp.Compiler.SourceCodeServices

[<AutoOpen>]
module FSharpErrors =
    // https://github.com/fsharp/FSharp.Compiler.Service/blob/9.0.0/src/fsharp/CompileOps.fs#L246
    // https://github.com/fsharp/FSharp.Compiler.Service/blob/9.0.0/src/fsharp/FSComp.txt
    let [<Literal>] UndefinedName = 39
    let [<Literal>] ModuleOrNamespaceRequired = 222
    let [<Literal>] UnrecognizedOption = 243
    let [<Literal>] UseBindingsIllegalInImplicitClassConstructors = 523
    let [<Literal>] UseBindingsIllegalInModules = 524
    let [<Literal>] UnusedValue = 1182

[<AbstractClass>]
type ErrorsStageProcessBase(fsFile, daemonProcess) =
    inherit FSharpDaemonStageProcessBase(fsFile, daemonProcess)

    let [<Literal>] UseKeywordLength = 3

    let document = daemonProcess.Document

    let getDocumentRange (error: FSharpErrorInfo) =
        if error.StartLineAlternate = 0 || error.ErrorNumber = ModuleOrNamespaceRequired then
            DocumentRange(document, TextRange(0, document.GetLineEndOffsetWithLineBreak(Line.O)))
        else
            let startOffset = document.GetDocumentOffset(error.StartLineAlternate - 1, error.StartColumn)
            let endOffset = document.GetDocumentOffset(error.EndLineAlternate - 1, error.EndColumn)
            DocumentRange(document, TextRange(startOffset, endOffset))

    abstract ShouldAddDiagnostic: error: FSharpErrorInfo * range: DocumentRange -> bool
    default x.ShouldAddDiagnostic(error: FSharpErrorInfo, range: DocumentRange) =
        error.ErrorNumber <> UnrecognizedOption

    member x.Execute(errors: FSharpErrorInfo[], committer: Action<DaemonStageResult>) =
        let createHighlighting (error: FSharpErrorInfo) (range: DocumentRange): IHighlighting =
            let message = error.Message

            match error.ErrorNumber with
            | UndefinedName -> UnresolvedHighlighting(message, range) :> _
            | UnusedValue -> UnusedHighlighting(message, range) :> _

            | UseBindingsIllegalInModules ->
                UseKeywordIllegalInModule(message, range.StartOffset.ExtendRight(UseKeywordLength)) :> _

            | UseBindingsIllegalInImplicitClassConstructors ->
                UseKeywordIllegalInPrimaryCtor(message, range.StartOffset.ExtendRight(UseKeywordLength)) :> _

            | _ ->

            match error.Severity with
            | FSharpErrorSeverity.Warning -> WarningHighlighting(message, range) :> _
            | _ -> ErrorHighlighting(message, range) :> _

        let highlightings = List(errors.Length)
        let errors =
            errors
            |> Array.map (fun error -> struct (error, getDocumentRange error))
            |> Array.distinct

        for struct (error, range) in errors  do
            if x.ShouldAddDiagnostic(error, range) then
                let highlighting = createHighlighting error range
                highlightings.Add(HighlightingInfo(highlighting.CalculateRange(), highlighting))
            x.SeldomInterruptChecker.CheckForInterrupt()

        committer.Invoke(DaemonStageResult(highlightings))
