﻿namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Stages

open System
open JetBrains.Application.Settings
open JetBrains.ProjectModel
open JetBrains.ReSharper.Daemon.Stages
open JetBrains.ReSharper.Feature.Services.InlayHints
open JetBrains.ReSharper.Feature.Services.TypeNameHints
open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.Naming
open JetBrains.ReSharper.Psi.Naming.Impl
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Psi.Util
open JetBrains.ReSharper.Feature.Services.Daemon
open JetBrains.ReSharper.Plugins.FSharp.Daemon.Cs.Stages
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open FSharp.Compiler.SourceCodeServices

type LocalReferencePatternVisitor(fsFile: IFSharpFile, highlightingContext: TypeNameHintHighlightingContext, namingPolicyProvider, nameParser) =
    inherit TreeNodeVisitor<IHighlightingConsumer>()

    let isTypeEvidentFromVariableNamePrefix (typ: IType) (variableNameParts: string[]) =
        if not (typ.IsBool()) then false else

        if variableNameParts.Length > 0 then
            let prefix = variableNameParts.[0].ToLowerInvariant()
            prefix = "has" || prefix = "is"
        else
            false

    let isEvidentFromVariableName (fsType: FSharpType) variableName =
        if not highlightingContext.HideTypeNameHintsWhenTypeNameIsEvidentFromVariableName then false else

        let typ = fsType.MapType(Array.empty, fsFile.GetPsiModule())
        if not (typ.IsValid()) then false else

        let variableNameParts = NamesHelper.GetParts(nameParser, namingPolicyProvider, variableName)
        if isTypeEvidentFromVariableNamePrefix typ variableNameParts then true else

        match typ.GetTypeElement() with
        | null -> false
        | typeElement ->

        let typeName = typeElement.ShortName
        String.Equals(typeName, variableName, StringComparison.OrdinalIgnoreCase) ||
        not (String.IsNullOrEmpty(typeName)) &&
        not (String.IsNullOrEmpty(variableName)) &&
        NamesHelper.IsLike(variableNameParts, NamesHelper.GetParts(nameParser, namingPolicyProvider, typeName))

    override x.VisitNode(node, context) =
        for child in node.Children() do
            match child with
            | :? IFSharpTreeNode as treeNode -> treeNode.Accept(x, context)
            | _ -> ()

    override x.VisitLocalReferencePat(localRefPat, consumer) =
        let pat = localRefPat.IgnoreParentParens()
        if isNotNull (TypedPatNavigator.GetByPattern(pat)) then () else

        let binding = BindingNavigator.GetByHeadPattern(pat)
        if isNotNull binding && isNotNull binding.ReturnTypeInfo then () else

        match box (localRefPat.GetFSharpSymbolUse()) with
        | null -> ()
        | symbolUse ->

        let symbolUse = symbolUse :?> FSharpSymbolUse
        match symbolUse.Symbol with
        | :? FSharpMemberOrFunctionOrValue as mfv when not (isEvidentFromVariableName mfv.FullType localRefPat.ReferenceName.Identifier.Name) ->
            let typeNameStr =
                symbolUse.DisplayContext.WithShortTypeNames(true)
                |> mfv.FullType.Format

            let range = localRefPat.GetNavigationRange().EndOffsetRange()

            // todo: TypeNameHintHighlighting can be used when RIDER-39605 is resolved
            consumer.AddHighlighting(TypeHintHighlighting(typeNameStr, range))
        | _ -> ()

type InferredTypeHintHighlightingProcess(fsFile, settings: IContextBoundSettingsStore, highlightingContext: TypeNameHintHighlightingContext, namingManager: NamingManager, nameParser: NameParser, daemonProcess) =
    inherit FSharpDaemonStageProcessBase(fsFile, daemonProcess)

    let namingPolicyProvider = namingManager.Policy.GetPolicyProvider(fsFile.Language, fsFile.GetSourceFile())

    let visitor = LocalReferencePatternVisitor(fsFile, highlightingContext, namingPolicyProvider, nameParser)

    let visitLetBindings (letBindings: ILetBindings) consumer =
        if not highlightingContext.ShowTypeNameHintsForImplicitlyTypedVariables then () else

        for binding in letBindings.Bindings do
            if highlightingContext.HideTypeNameHintsForImplicitlyTypedVariablesWhenTypeIsEvident && isTypeEvident binding.Expression then () else
            binding.HeadPattern.Accept(visitor, consumer)

    override x.Execute(committer) =
        let consumer = FilteringHighlightingConsumer(daemonProcess.SourceFile, fsFile, settings)
        fsFile.ProcessThisAndDescendants(Processor(x, consumer))
        committer.Invoke(DaemonStageResult(consumer.Highlightings))

    override x.VisitLetModuleDecl(moduleDecl, consumer) =
        visitLetBindings moduleDecl consumer

    override x.VisitLetOrUseExpr(letOrUseExpr, consumer) =
        visitLetBindings letOrUseExpr consumer

    override x.VisitMemberParamDeclaration(paramDecl, consumer) =
        // todo: do we need another setting for this?
        if highlightingContext.ShowTypeNameHintsForImplicitlyTypedVariables then
            paramDecl.Pattern.Accept(visitor, consumer)

    override x.VisitMatchClause(matchClause, consumer) =
        if highlightingContext.ShowTypeNameHintsForVarDeclarationsInPatternMatchingExpressions then
            matchClause.Pattern.Accept(visitor, consumer)

    override x.VisitLambdaExpr(lambdaExpr, consumer) =
        if not highlightingContext.ShowTypeNameHintsForLambdaExpressionParameters then () else

        for pattern in lambdaExpr.Patterns do
            pattern.Accept(visitor, consumer)

[<DaemonStage(StagesBefore = [| typeof<GlobalFileStructureCollectorStage> |])>]
type InferredTypeHintStage(namingManager: NamingManager, nameParser: NameParser) =
    inherit FSharpDaemonStageBase()

    override x.IsSupported(sourceFile, processKind) =
        processKind = DaemonProcessKind.VISIBLE_DOCUMENT &&
        base.IsSupported(sourceFile, processKind) &&
        not (sourceFile.LanguageType.Is<FSharpSignatureProjectFileType>())

    override x.CreateStageProcess(fsFile, settings, daemonProcess) =
        let highlightingContext = TypeNameHintHighlightingContext(settings)
        let isEnabled =
            highlightingContext.ShowTypeNameHintsForImplicitlyTypedVariables ||
            highlightingContext.ShowTypeNameHintsForLambdaExpressionParameters ||
            highlightingContext.ShowTypeNameHintsForVarDeclarationsInPatternMatchingExpressions
        
        if not isEnabled then null else
        InferredTypeHintHighlightingProcess(fsFile, settings, highlightingContext, namingManager, nameParser, daemonProcess) :> _
