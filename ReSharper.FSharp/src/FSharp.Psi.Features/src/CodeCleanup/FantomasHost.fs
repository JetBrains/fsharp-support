namespace JetBrains.ReSharper.Plugins.FSharp.Services.Formatter

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Text
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.Rd.Tasks
open JetBrains.ReSharper.Plugins.FSharp.Fantomas.Client
open JetBrains.ReSharper.Plugins.FSharp.Fantomas.Protocol

module internal Reflection =
    let formatSettingType = typeof<FSharpFormatSettingsKey>

    let getFieldValue obj fieldName defaultValue =
        let field = formatSettingType.GetField(fieldName)
        if isNotNull field then field.GetValue(obj) else defaultValue


[<SolutionComponent>]
type FantomasHost(solution: ISolution, fantomasFactory: FantomasProcessFactory) =
    let mutable connection: FantomasConnection = null
    let mutable formatConfigFields: string[] = [||]

    let isConnectionAlive () =
        isNotNull connection && connection.IsActive

    let connect () =
        if isConnectionAlive () then () else
        let formatterHostLifetime = Lifetime.Define(solution.GetLifetime())
        connection <- fantomasFactory.Create(formatterHostLifetime.Lifetime).Run()
        formatConfigFields <- connection.Execute(fun x -> connection.ProtocolModel.GetFormatConfigFields.Sync(JetBrains.Core.Unit.Instance))

    let convertRange (range: range) =
        RdFcsRange(range.FileName, range.StartLine, range.StartColumn, range.EndLine, range.EndColumn)

    let convertFormatSettings (settings: FSharpFormatSettingsKey) =
        [| for field in formatConfigFields ->
            let fieldName =
                match field with
                    | "IndentSize" -> "INDENT_SIZE"
                    | "MaxLineLength" -> "WRAP_LIMIT"
                    | x -> x
            (Reflection.getFieldValue settings fieldName "").ToString() |]

    let convertParsingOptions (options: FSharpParsingOptions) =
        let lightSyntax = Option.toNullable options.LightSyntax
        RdFcsParsingOptions(Array.last options.SourceFiles, lightSyntax,
            List.toArray options.ConditionalCompilationDefines, options.IsExe)

    member x.FormatSelection(filePath, range, source, settings, options, newLineText) =
        let args =
            RdFormatSelectionArgs(convertRange range, filePath, source, convertFormatSettings settings,
                convertParsingOptions options, newLineText)

        connection.Execute(fun () -> connection.ProtocolModel.FormatSelection.Sync(args, RpcTimeouts.Maximal))

    member x.FormatDocument(filePath, source, settings, options, newLineText) =
        let args =
            RdFormatDocumentArgs(filePath, source, convertFormatSettings settings, convertParsingOptions options,
                newLineText)

        connection.Execute(fun () -> connection.ProtocolModel.FormatDocument.Sync(args, RpcTimeouts.Maximal))
