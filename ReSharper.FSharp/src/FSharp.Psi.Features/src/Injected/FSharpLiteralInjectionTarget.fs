namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Injected

open System
open System.Text.RegularExpressions
open JetBrains.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Psi.CSharp.Util.Literals
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.StringLiteralsUtil
open JetBrains.ReSharper.Psi.RegExp.ClrRegex
open JetBrains.ReSharper.Psi.RegExp.ClrRegex.Parsing
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Psi.Impl.Shared.InjectedPsi
open JetBrains.Text

[<SolutionComponent>]
type FSharpLiteralInjectionTarget() =
    interface IInjectionTargetLanguage with
        override __.ShouldInjectByAnnotation(originalNode, prefix, postfix) =
            prefix <- null
            postfix <- null
            false

        override __.GetStartOffsetForString(originalNode) =
            match originalNode.GetTokenType().GetLiteralType() with
            | FSharpLiteralType.VerbatimString -> 2
            | _ -> 1

        override __.GetEndOffsetForString(originalNode) = 1

        override __.UpdateNode(generatedFile, generatedNode, originalNode, length, prefix, postfix, startOffset, endOffset) =
            length <- -1
            null

        override __.SupportsRegeneration = false

        override __.IsInjectionAllowed(literalNode) =
            let tokenType = literalNode.GetTokenType()
            if isNull tokenType || not tokenType.IsStringLiteral then false else

            match tokenType.GetLiteralType() with
            | FSharpLiteralType.VerbatimString
            | FSharpLiteralType.RegularString -> true
            | _ -> false

        override __.GetCorrespondingCommentTextForLiteral(originalNode) = null

        override __.CreateBuffer(originalNode, text, options) =
            let literalType =
                if text.StartsWith("@", StringComparison.Ordinal) then CSharpLiteralType.VerbatimString
                else CSharpLiteralType.RegularString

            let lexerOptions =
                match options with
                | :? RegexOptions as regexOptions when regexOptions.HasFlag(RegexOptions.IgnorePatternWhitespace) ->
                    ClrRegexLexerOptions(true)
                | _ ->
                    ClrRegexLexerOptions.None

            CSharpRegExpBuffer(StringBuffer(text), literalType, lexerOptions) :> _

        override __.DoNotProcessNodeInterior(element) = false

        override __.IsPrimaryLanguageApplicable(sourceFile) =
            match sourceFile.LanguageType with
            | :? FSharpProjectFileType -> true
            | _ -> false

        override __.CreateLexerFactory(languageService) =
            languageService.GetPrimaryLexerFactory()

        override __.AllowsLineBreaks(literalNode) =
            match literalNode.GetTokenType().GetLiteralType() with
            | FSharpLiteralType.VerbatimString -> true
            | _ -> false

        override __.IsWhitespaceToken(token) =
            token.GetTokenType().IsWhitespace

        override x.FixValueRangeForLiteral(element) =
            let startOffset = (x :> IInjectionTargetLanguage).GetStartOffsetForString(element)
            element.GetTreeTextRange().TrimLeft(startOffset).TrimRight(1)

        override __.Language = FSharpLanguage.Instance :> _
