namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.LanguageService

open JetBrains.Diagnostics
open JetBrains.DocumentModel
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Plugins.FSharp.Psi.Util
open JetBrains.ReSharper.Psi.CodeStyle
open JetBrains.ReSharper.Psi.ExtensionsAPI.Tree
open JetBrains.ReSharper.Psi.Modules
open JetBrains.ReSharper.Psi.Naming
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Resources.Shell

type FSharpElementFactory(languageService: IFSharpLanguageService, psiModule: IPsiModule) =
    let [<Literal>] moniker = "F# element factory"

    let namingService = NamingManager.GetNamingLanguageService(FSharpLanguage.Instance)

    let createDocument source =
        let documentFactory = Shell.Instance.GetComponent<IInMemoryDocumentFactory>()
        documentFactory.CreateSimpleDocumentFromText(source, moniker)

    let createFile source =
        let document = createDocument source
        let parser = languageService.CreateParser(document)

        let fsFile = parser.ParseFSharpFile(StandaloneDocument = document)
        SandBox.CreateSandBoxFor(fsFile, psiModule)
        fsFile

    let getModuleDeclaration source =
        let fsFile = createFile source
        fsFile.ModuleDeclarations.First()

    let getModuleMember source =
        let moduleDeclaration = getModuleDeclaration source
        moduleDeclaration.Members.First()

    let getDoDecl source =
        let moduleMember = getModuleMember source
        moduleMember.As<IDo>().NotNull()

    let getExpression source =
        let doDecl = getDoDecl source
        doDecl.Expression.NotNull()

    let createAppExpr addSpace =
        let space = if addSpace then " " else ""
        let source = sprintf "()%s()" space
        getExpression source :?> IPrefixAppExpr

    let createLetBinding bindingName =
        let source = sprintf "do (let %s = ())" bindingName
        let newExpr = getExpression source
        newExpr.As<IParenExpr>().InnerExpression.As<ILetOrUseExpr>()
        
    let createMemberBinding bindingName typeParameters argsSource =
        let typeArgsSource =
            match typeParameters with
            | [] -> ""
            | parameters -> sprintf "<%s>" (parameters |> List.map(sprintf "'%s") |> String.concat ",")
        let source = sprintf "type DummyType() = \n member this.%s%s %s= failwith \"todo\"" bindingName typeArgsSource argsSource
        let moduleMember = getModuleMember source
        let typeDecl = moduleMember.As<ITypeDeclarationGroup>().TypeDeclarations |> Seq.exactlyOne
        let objectTypeDecl = typeDecl.As<IObjectTypeDeclaration>()
        (objectTypeDecl.TypeMembers
         |> Seq.where (function | :? IMemberDeclaration -> true | _ -> false)
         |> Seq.exactlyOne).As<IMemberDeclaration>()

    let createParenExpr (expr: IFSharpExpression) =
        let parenExpr = getExpression "(())" :?> IParenExpr
        ModificationUtil.ReplaceChild(parenExpr.InnerExpression, expr.Copy()) |> ignore
        parenExpr
    
    let createAttributeList attrName: IAttributeList =
            let source = sprintf "[<%s>] ()" attrName
            let doDecl = getDoDecl source
            doDecl.AttributeLists.[0]

    interface IFSharpElementFactory with
        member x.CreateOpenStatement(ns) =
            // todo: mangle ns
            let source = "open " + ns
            let moduleDeclaration = getModuleDeclaration source

            moduleDeclaration.Members.First() :?> _

        member x.CreateWildPat() =
            let source = "let _ = ()"
            let moduleDeclaration = getModuleDeclaration source

            let letModuleDecl = moduleDeclaration.Members.First().As<ILetModuleDecl>()
            let binding = letModuleDecl.Bindings.First()
            binding.HeadPattern :?> _

        member x.CreateIgnoreApp(expr, newLine) =
            let source = "() |> ignore"

            let indent = expr.Indent
            let newExpr = getExpression source

            match newExpr.As<IBinaryAppExpr>() with
            | null -> failwith "Could not get outer appExpr"
            | binaryAppExpr ->

            let expr = ModificationUtil.ReplaceChild(binaryAppExpr.LeftArgument, expr.Copy())

            if newLine then
                ModificationUtil.ReplaceChild(expr.NextSibling, Whitespace(indent)) |> ignore
                ModificationUtil.AddChildBefore(expr.NextSibling, NewLine(expr.GetLineEnding())) |> ignore

            binaryAppExpr

        member x.CreateRecordExprBinding(field, addSemicolon) =
            let field = namingService.MangleNameIfNecessary(field)
            let semicolon = if addSemicolon then ";" else ""

            let source = sprintf """{ %s = failwith "todo"%s }""" field semicolon
            let newExpr = getExpression source

            match newExpr.As<IRecordExpr>() with
            | null -> failwith "Could not get record expr"
            | recordExpr -> recordExpr.ExprBindings.First()

        member x.CreateAppExpr(funcName, argExpr) =
            let source = sprintf "%s ()" funcName
            let newExpr = getExpression source :?> IPrefixAppExpr
            let newArg = newExpr.SetArgumentExpression(argExpr.Copy())
            addParensIfNeeded newArg |> ignore
            newExpr

        member x.CreateAppExpr(addSpace) =
            createAppExpr addSpace

        member x.CreateAppExpr(funExpr, argExpr, addSpace) =
            let appExpr = createAppExpr addSpace
            appExpr.SetFunctionExpression(funExpr.Copy()) |> ignore
            appExpr.SetArgumentExpression(argExpr.Copy()) |> ignore
            appExpr

        member x.CreateLetBindingExpr(bindingName) =
            createLetBinding bindingName

        member x.CreateLetModuleDecl(bindingName) =
            let source = sprintf "let %s = ()" bindingName
            getModuleMember source :?> ILetModuleDecl
            
        member x.CreateMemberParamDeclarations(curriedParameterNames, isSpaceAfterComma) : IMemberParamDeclaration list =
            let spaceAfterComma = if isSpaceAfterComma then " " else ""
            let argsSource =
                curriedParameterNames
                |> List.fold (fun state parameters ->
                    let paramGroupString =
                        match parameters with
                        | [] -> "()" // No arguments case
                        | [singleParam] -> singleParam
                        | multipleParams -> multipleParams |> String.concat ("," + spaceAfterComma) |> sprintf "(%s)"
                    state  + " " + paramGroupString
                    ) ""
            let memberBinding = createMemberBinding "fakeBindingName" List.empty argsSource
            memberBinding.Parameters |> Seq.toList
            
        member x.CreateMemberBindingExpr(bindingName, typeParameters, args) : IMemberDeclaration =
            let fakeArgNames = "dummy " |> String.replicate (args.Length)
            let memberBinding = createMemberBinding bindingName (Seq.toList typeParameters) fakeArgNames
            for realArg, fakeArg in Seq.zip args memberBinding.Parameters do
                ModificationUtil.ReplaceChild(fakeArg, realArg) |> ignore
            memberBinding
            
        member x.CreateInterfaceImplementation(typeReferenceName, memberDeclarations, baseIndent) : IInterfaceImplementation =
            let memberIndent = baseIndent + typeReferenceName.FSharpFile.GetIndentSize()
            let dummyMembers = "  member this.DummyMember() = ()" |> String.replicate (memberDeclarations.Length)
            let source = sprintf "type DummyType() =\n  interface DummyInterface with %s" dummyMembers
            
            let moduleMember = getModuleMember source
            let typeDecl = moduleMember.As<ITypeDeclarationGroup>().TypeDeclarations |> Seq.exactlyOne
            let objectTypeDecl = typeDecl.As<IObjectTypeDeclaration>()
            let interfaceImplementation = objectTypeDecl.TypeMembers.OfType<IInterfaceImplementation>() |> Seq.exactlyOne
            
            ModificationUtil.ReplaceChild(interfaceImplementation.TypeName, typeReferenceName) |> ignore
            for dummyMember, realMember in Seq.zip interfaceImplementation.TypeMembers memberDeclarations do 
                let inPlaceMember = ModificationUtil.ReplaceChild(dummyMember, realMember)
                let whitespace = ModificationUtil.ReplaceChild(inPlaceMember.PrevSibling, Whitespace(memberIndent))
                ModificationUtil.AddChildBefore(whitespace, NewLine(typeReferenceName.GetLineEnding())) |> ignore
            
            interfaceImplementation

        member x.CreateConstExpr(text) =
            getExpression text :?> _

        member x.CreateMatchExpr(expr) =
            let source = "match () with | _ -> ()"

            let indent = expr.Indent
            let newExpr = getExpression source

            match newExpr.As<IMatchExpr>() with
            | null -> failwith "Could not get outer appExpr"
            | matchExpr ->

            match matchExpr.Clauses.[0].As<IMatchClause>() with
            | null -> failwith "Could not get inner appExpr"
            | matchClause ->

            let expr = ModificationUtil.ReplaceChild(matchExpr.Expression, expr.Copy())

            let whitespace = ModificationUtil.ReplaceChild(matchClause.PrevSibling, Whitespace(indent))
            ModificationUtil.AddChildBefore(whitespace, NewLine(expr.GetLineEnding())) |> ignore

            matchExpr

        member x.CreateMatchClause() =
            let source = "match () with | _ -> failwith \"todo\""

            let matchExpr = getExpression source :?> IMatchExpr
            matchExpr.Clauses.[0]

        member x.CreateParenExpr() =
            getExpression "(())" :?> _

        member x.CreateParenExpr(expr) =
            createParenExpr expr

        member x.AsReferenceExpr(typeReference: ITypeReferenceName) =
            getExpression (typeReference.GetText()) :?> _

        member x.CreateReferenceExpr(name) =
            let source = sprintf "do %s" name
            let newExpr = getExpression source :?> IReferenceExpr
            newExpr :> _

        member x.CreateForEachExpr(expr) =
            let sourceFile = expr.GetSourceFile()
            let indent = expr.Indent + sourceFile.GetFormatterSettings(expr.Language).INDENT_SIZE

            let forExpr = getExpression "for _ in () do ()" :?> IForEachExpr

            let expr = ModificationUtil.ReplaceChild(forExpr.InClause, expr.Copy())
            let whitespace = ModificationUtil.ReplaceChild(forExpr.DoExpression.PrevSibling, Whitespace(indent))
            ModificationUtil.AddChildBefore(whitespace, NewLine(expr.GetLineEnding())) |> ignore

            forExpr

        member x.CreateExpr(text) =
            getExpression text

        member x.CreateBinaryAppExpr(text, arg1: IFSharpExpression, arg2: IFSharpExpression) =
            let source = "() " + text + " ()"
            let expr = getExpression source
            let appExpr = expr :?> IBinaryAppExpr

            let leftArg = ModificationUtil.ReplaceChild(appExpr.LeftArgument, arg1.IgnoreInnerParens())
            addParensIfNeeded leftArg |> ignore

            let rightArg = ModificationUtil.ReplaceChild(appExpr.RightArgument, arg2.IgnoreInnerParens())
            addParensIfNeeded rightArg |> ignore

            expr

        member x.CreateSetExpr(left: IFSharpExpression, right: IFSharpExpression) =
            let source = "() <- ()"
            let expr = getExpression source
            let setExpr = expr :?> ISetExpr

            let leftArg = ModificationUtil.ReplaceChild(setExpr.LeftExpression, left.IgnoreInnerParens())
            addParensIfNeeded leftArg |> ignore

            let rightArg = ModificationUtil.ReplaceChild(setExpr.RightExpression, right.IgnoreInnerParens())
            addParensIfNeeded rightArg |> ignore

            expr
  

        member x.CreateEmptyAttributeList() =
            let attributeList = createAttributeList "Foo"
            ModificationUtil.DeleteChild(attributeList.Attributes.[0])
            attributeList

        member x.CreateAttribute(attrName) =
            let attributeList = createAttributeList attrName
            attributeList.Attributes.[0]
