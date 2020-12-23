namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes

open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Resources.Shell
open FSharp.Compiler.SourceCodeServices
open JetBrains.ReSharper.Plugins.FSharp.Psi.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl

type AddDiscriminatedUnionAllClauses(warning: MatchIncompleteWarning) =
    inherit FSharpQuickFixBase()

    let unionNameFromCase (pattern : IFSharpPattern) : string option =
        match pattern with
        | :? INamedPat as pat1 -> Some pat1.Identifier.Name
        | _ -> None
    let rec patternIsRestrictive (pattern: IFSharpPattern) (fsharpType: FSharpType) : bool =
        if fsharpType.TypeDefinition.IsFSharpUnion && pattern.As<INamedPat>() |> isNotNull then
            let unionLiteral =
               fsharpType.TypeDefinition.UnionCases
               |> Seq.tryFind (fun unionCase -> unionCase.DisplayName = (unionNameFromCase pattern).Value)
            match unionLiteral with
            | Some literal ->
                match literal.UnionCaseFields |> Seq.toList with
                | [] -> false
                | caseFields ->
                    // If there are more than case fields for the DU type than are included in the match statement
                    // parameters then assume that the match is incomplete. Really case fields need to be matched to
                    // parameters in the match statement, this is purely a heuristic for now.
                    let parameters = pattern.As<IParametersOwnerPat>().Parameters |> Seq.exactlyOne
                    let patterns = parameters.GetInnerMatchStatementPatterns() |> Seq.toList
                    if caseFields.Length > patterns.Length then
                        match patterns with
                        | [wildPatParam] when wildPatParam.As<IWildPat>() |> isNotNull -> false
                        | _ -> true
                    else
                        List.zip patterns caseFields
                            |> List.tryFind(fun (pattern, field) -> patternIsRestrictive pattern field.FieldType)
                            |> Option.isSome
            | None -> false
        else
            // If you're not a union case, if you're anything other than a free variable (i.e. INamedPat) or
            // '_' (i.e. IWildPat), then assume that case is restrictive.
            pattern.As<IWildPat>() |> isNull && pattern.As<INamedPat>() |> isNull
                
    let caseIsRestrictive (clause: IMatchClause) (unionCase: FSharpUnionCase) =
        let whenClauseExists = clause.WhenExpression |> isNotNull
        let restrictivePattern = patternIsRestrictive clause.Pattern unionCase.ReturnType
        whenClauseExists || restrictivePattern
        
    let isCaseNonExhaustive (unionCase: FSharpUnionCase) (relevantMatchClauses: IMatchClause list) : bool =
        // By definition, if there's a single non-restrictive case, the match is exhaustive
        relevantMatchClauses
        |> List.tryFind(fun clause -> caseIsRestrictive clause unionCase |> not)
        |> Option.isNone
    
    let nonExhaustiveUnionCases (unionCases : FSharpUnionCase seq) (allMatchClauses : IMatchClause seq) : FSharpUnionCase list =
        let allMatchClauses = allMatchClauses |> Seq.toList
        
        unionCases
        |> Seq.map (fun case ->
            (case,
             allMatchClauses
             |> List.filter (fun clause -> case.DisplayName = (unionNameFromCase clause.Pattern).Value)))
        |> Seq.filter (fun (case, clauses) -> isCaseNonExhaustive case clauses)
        |> Seq.map fst
        |> Seq.toList
    
    override x.Text = "Autogenerate all discriminated union cases"

    override x.IsAvailable _ =
        let fsharpType = warning.Expr.Expression.TryGetFcsType()
        
        isValid warning.Expr
        && (isNotNull fsharpType)
        && fsharpType.HasTypeDefinition
        && fsharpType.TypeDefinition.IsFSharpUnion
        && (nonExhaustiveUnionCases fsharpType.TypeDefinition.UnionCases warning.Expr.Clauses |> List.isEmpty |> not)

    override x.ExecutePsiTransaction _ =
        let expr = warning.Expr
        use writeCookie = WriteLockCookie.Create(expr.IsPhysical())
        let factory = expr.CreateElementFactory()
        use enableFormatter = FSharpRegistryUtil.AllowFormatterCookie.Create()

        let typeDefinition = warning.Expr.Expression.TryGetFcsType().TypeDefinition
                
        let nodesToAdd =
            nonExhaustiveUnionCases typeDefinition.UnionCases warning.Expr.Clauses
            |> Seq.collect(fun case ->
                [NewLine(expr.GetLineEnding()) :> ITreeNode
                 factory.CreateMatchClause(typeDefinition.DisplayName + "." + case.DisplayName, case.HasFields) :> ITreeNode])
            |> Seq.toList
        
        addNodesAfter expr.WithKeyword nodesToAdd |> ignore
