namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.CodeCompletion

open System
open JetBrains.DocumentModel
open JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems
open JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure.LookupItems.Impl
open JetBrains.ReSharper.Feature.Services.Lookup
open JetBrains.ReSharper.Feature.Services.ParameterInfo
open JetBrains.ReSharper.Plugins.FSharp.Common.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Plugins.FSharp.Services.Cs.CodeCompletion
open JetBrains.Text
open JetBrains.UI.Icons
open JetBrains.UI.RichText
open JetBrains.Util
open Microsoft.FSharp.Compiler.SourceCodeServices

type FSharpLookupCandidateInfo =
    {
        Description: string
        XmlDoc: FSharpXmlDoc
    }

type FSharpLookupAdditionalInfo =
    {
        Icon: IconId
        ReturnType: string
    }

type FSharpLookupCandidate(info: FSharpLookupCandidateInfo, xmlDocService: FSharpXmlDocService) =
    interface ICandidate with
        member x.GetSignature(_, _, _, _, _) = RichText(info.Description)
        member x.GetDescription() = xmlDocService.GetXmlDoc(info.XmlDoc)
        member x.Matches(_) = true

        member x.GetParametersInfo(_, _) = ()
        member x.PositionalParameterCount = 0
        member x.IsObsolete = false
        member x.ObsoleteDescription = null
        member val IsFilteredOut = false with get, set

type FSharpErrorLookupItem(item: FSharpDeclarationListItem<FSharpLookupAdditionalInfo>) =
    inherit TextLookupItemBase()

    override x.Image = null
    override x.Text = item.NameInCode
    override x.Accept(_, _, _, _, _, _) = ()

    interface IDescriptionProvidingLookupItem with
        member x.GetDescription() =
            let (FSharpToolTipText(tooltips)) = item.DescriptionTextAsync.RunAsTask()
            tooltips
            |> List.tryHead
            |> Option.bind (function | FSharpToolTipElement.CompositionError e -> Some (RichTextBlock(e)) | _ -> None)
            |> Option.toObj

type FSharpLookupItem(item: FSharpDeclarationListItem<FSharpLookupAdditionalInfo>, context: FSharpCodeCompletionContext,
                      xmlDocService: FSharpXmlDocService) =
    inherit TextLookupItemBase()

    let candidates = lazy (let (FSharpToolTipText(tooltips)) = item.DescriptionTextAsync.RunAsTask()
        tooltips |> List.map (function
            | FSharpToolTipElement.Group(overloads) ->
                overloads |> List.map (fun o -> { Description = o.MainDescription; XmlDoc = o.XmlDoc })
            | FSharpToolTipElement.CompositionError e -> [{ Description = e; XmlDoc = FSharpXmlDoc.None }]
            | _ -> [])
        |> List.concat)

    override x.Image = item.AdditionalInfo |> Option.map (fun i -> i.Icon) |> Option.toObj
    override x.Text = item.NameInCode

    override x.Accept(textControl, nameRange, insertType, suffix, solution, keepCaret) =
        base.Accept(textControl, nameRange, insertType, suffix, solution, keepCaret)

        // todo: move it to a separate type (and reuse in open namespace Quick Fix)
        if item.NamespaceToOpen.IsSome then
            let line = int context.Coords.Line + 1
            let fullName = item.FullName.Split('.')
            let parseTree = (context.BasicContext.File :?> IFSharpFile).ParseResults.Value.ParseTree.Value
            let insertionPoint = OpenStatementInsertionPoint.Nearest

            match ParsedInput.tryFindNearestPointToInsertOpenDeclaration line parseTree fullName insertionPoint with
            | Some context ->
                let document = textControl.Document
                let getLineText = fun lineNumber -> document.GetLineText(docLine lineNumber)
                let pos = context |> ParsedInput.adjustInsertionPoint getLineText

                let isSystem = item.NamespaceToOpen.Value.StartsWith("System.")
                let openPrefix = String(' ', pos.Column) + "open "
                let textToInsert = openPrefix + item.NamespaceToOpen.Value

                let line = pos.Line - 1 |> max 0
                let lineToInsert =
                    seq { line - 1 .. -1 .. 0 }
                    |> Seq.takeWhile (fun i ->
                        let lineText = document.GetLineText(docLine i)
                        lineText.StartsWith(openPrefix) &&
                        (textToInsert < lineText || isSystem && not (lineText.StartsWith("open System")))) // todo: System<smth> namespaces
                    |> Seq.tryLast
                    |> Option.defaultValue line

                // add empty line after all open expressions if needed
                let insertEmptyLine = not (document.GetLineText(docLine line).IsNullOrWhitespace())

                let prevLineEndOffset =
                    if lineToInsert > 0 then document.GetLineEndOffsetWithLineBreak(docLine (max 0 (lineToInsert - 1)))
                    else 0

                document.InsertText(prevLineEndOffset, textToInsert + "\n" + (if insertEmptyLine then "\n" else ""))
            | _ -> ()

    override x.GetDisplayName() =
        let name = LookupUtil.FormatLookupString(item.Name, x.TextColor)
        item.NamespaceToOpen
        |> Option.iter (fun ns -> LookupUtil.AddInformationText(name, "(in " + ns + ")", itemInfoTextStyle))
        name

    interface IParameterInfoCandidatesProvider with
        member x.HasCandidates =
            candidates.Value.Length > 1 ||
            item.Kind |> function CompletionItemKind.Method _ -> true  | _ -> false

        member x.CreateCandidates() =
            candidates.Value |> List.map (fun i -> FSharpLookupCandidate(i, xmlDocService) :> ICandidate) :>_

    interface IDescriptionProvidingLookupItem with
        /// called when x.HasCandidates is false
        member x.GetDescription() =
            match List.tryHead candidates.Value with
            | Some item ->
                let isNullOrWhiteSpace = RichTextBlock.IsNullOrWhiteSpace

                let mainDescription = RichTextBlock(item.Description)
                match xmlDocService.GetXmlDoc(item.XmlDoc) with
                | null -> ()
                | xmlDoc ->
                    if not (isNullOrWhiteSpace mainDescription || isNullOrWhiteSpace xmlDoc) then
                        mainDescription.AddLines(RichTextBlock(" "))
                    mainDescription.AddLines(xmlDoc)
                mainDescription
            | _ -> null
