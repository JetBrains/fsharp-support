namespace JetBrains.ReSharper.Plugins.FSharp.Psi.LanguageService

open JetBrains.DocumentModel
open JetBrains.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp.Checker
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2
open JetBrains.ReSharper.Plugins.FSharp.Psi.LanguageService.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Psi.Resolve
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Plugins.FSharp.Services.Formatter
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.CSharp.Impl
open JetBrains.ReSharper.Psi.Impl
open JetBrains.ReSharper.Psi.Modules
open JetBrains.ReSharper.Psi.Parsing
open JetBrains.ReSharper.Psi.Tree
open JetBrains.Util

[<Language(typeof<FSharpLanguage>)>]
type FSharpLanguageService
        (languageType, constantValueService, cacheProvider: FSharpCacheProvider, checkerService: FSharpCheckerService,
         formatter: FSharpDummyCodeFormatter) =
    inherit LanguageService(languageType, constantValueService)

    let lexerFactory = FSharpLexerFactory()

    let getSymbolsCache (psiModule: IPsiModule) =
        psiModule.GetSolution().GetComponent<IFSharpResolvedSymbolsCache>()

    override x.IsCaseSensitive = true
    override x.SupportTypeMemberCache = true
    override x.CacheProvider = cacheProvider :> _

    override x.GetPrimaryLexerFactory() = lexerFactory :> _
    override x.CreateFilteringLexer(lexer) = lexer

    override x.CreateParser(lexer, _, sourceFile) =
        FSharpParser(lexer, sourceFile, checkerService, getSymbolsCache sourceFile.PsiModule) :> _

    member x.CreateParser(document: IDocument, psiModule: IPsiModule) =
        let lexer = TokenBuffer(lexerFactory.CreateLexer(document.Buffer)).CreateLexer()
        FSharpParser(lexer, document, checkerService, getSymbolsCache psiModule) :> IParser

    override x.IsTypeMemberVisible(typeMember) =
        match typeMember with
        | :? IFSharpTypeMember as fsTypeMember -> fsTypeMember.IsVisibleFromFSharp
        | _ -> true

    override x.TypePresenter = CLRTypePresenter.Instance
    override x.DeclaredElementPresenter = CSharpDeclaredElementPresenter.Instance :> _ // todo: implement F# presenter

    override x.CodeFormatter = formatter :> _
    override x.FindTypeDeclarations(_) = EmptyList.Instance :> _

    override x.CanContainCachableDeclarations(node) =
        not (node :? IExpression)

    override x.CalcOffset(declaration) =
        match declaration with
        | :? INamedPat as namedPat -> namedPat.GetOffset()
        | _ -> base.CalcOffset(declaration)

    override x.GetReferenceAccessType(_,reference) =
        match reference.As<FSharpSymbolReference>() with
        | null -> ReferenceAccessType.OTHER
        | symbolReference ->

        match symbolReference.GetElement().As<IReferenceExpression>() with
        | null -> ReferenceAccessType.OTHER
        | referenceExpression ->

        match referenceExpression.IdentifierToken with
        | null -> ReferenceAccessType.OTHER
        | referenceToken ->

        match referenceExpression.GetContainingNode<ISetExpr>() with
        | null -> ReferenceAccessType.OTHER
        | setExpr ->

        match setExpr.ReferenceIdentifier with
        | token when token == referenceToken -> ReferenceAccessType.WRITE
        | _ -> ReferenceAccessType.OTHER

    override x.CreateElementPointer(declaredElement) =
        match declaredElement.As<IFSharpGeneratedFromOtherElement>() with
        | null -> null
        | generatedElement -> generatedElement.CreatePointer() :?> _

    override x.AnalyzePossibleInfiniteInheritance = false
