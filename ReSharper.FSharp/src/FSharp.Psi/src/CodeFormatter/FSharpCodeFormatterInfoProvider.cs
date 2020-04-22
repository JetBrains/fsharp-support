using System.Linq;
using JetBrains.Application.Settings;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.FSharp.Psi;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.CodeStyle;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.FSharp.Services.Formatter
{
  [Language(typeof(FSharpLanguage))]
  public class FSharpFormatterInfoProvider :
    FormatterInfoProviderWithFluentApi<CodeFormattingContext, FSharpFormatSettingsKey>
  {
    public FSharpFormatterInfoProvider(ISettingsSchema settingsSchema) : base(settingsSchema)
    {
      var bindingAndModuleDeclIndentingRulesParameters = new[]
      {
        ("NestedModuleDeclaration", ElementType.NESTED_MODULE_DECLARATION, NestedModuleDeclaration.MODULE_MEMBER),
        ("TopBinding", ElementType.TOP_BINDING, TopBinding.CHAMELEON_EXPR),
        ("LocalBinding", ElementType.LOCAL_BINDING, LocalBinding.EXPR),
        ("LetModuleDeclBinding", ElementType.LET_MODULE_DECL, LetModuleDecl.BINDING),
        ("LetExprBinding", ElementType.LET_OR_USE_EXPR, LetOrUseExpr.BINDING),
        ("NestedModuleDeclName", ElementType.NESTED_MODULE_DECLARATION, NestedModuleDeclaration.IDENTIFIER),
        ("NamedModuleDeclName", ElementType.NAMED_MODULE_DECLARATION, NamedModuleDeclaration.IDENTIFIER),
      };

      var synExprIndentingRulesParameters = new[]
      {
        ("ForExpr", ElementType.FOR_EXPR, ForExpr.DO_EXPR),
        ("ForEachExpr", ElementType.FOR_EACH_EXPR, ForEachExpr.DO_EXPR),
        ("WhileExpr", ElementType.WHILE_EXPR, WhileExpr.DO_EXPR),
        ("DoExpr", ElementType.DO_EXPR, DoExpr.EXPR),
        ("AssertExpr", ElementType.ASSERT_EXPR, AssertExpr.EXPR),
        ("LazyExpr", ElementType.LAZY_EXPR, LazyExpr.EXPR),
        ("ComputationExpr", ElementType.COMPUTATION_EXPR, ComputationExpr.EXPR),
        ("SetExpr", ElementType.SET_EXPR, SetExpr.RIGHT_EXPR),
        ("TryFinally_TryExpr", ElementType.TRY_FINALLY_EXPR, TryFinallyExpr.TRY_EXPR),
        ("TryFinally_FinallyExpr", ElementType.TRY_FINALLY_EXPR, TryFinallyExpr.FINALLY_EXPR),
        ("TryWith_TryExpr", ElementType.TRY_WITH_EXPR, TryWithExpr.TRY_EXPR),
        ("IfThenExpr", ElementType.IF_THEN_ELSE_EXPR, IfThenElseExpr.THEN_EXPR),
        ("ElifThenExpr", ElementType.ELIF_EXPR, ElifExpr.THEN_EXPR),
        ("LambdaExprBody", ElementType.LAMBDA_EXPR, LambdaExpr.EXPR),
      };

      var typeDeclarationIndentingRulesParameters = new[]
      {
        ("EnumDeclaration", ElementType.ENUM_DECLARATION, EnumDeclaration.ENUM_MEMBER),
        ("UnionDeclarationCases", ElementType.UNION_DECLARATION, UnionDeclaration.UNION_REPR),
        ("TypeAbbreviation", ElementType.TYPE_ABBREVIATION_DECLARATION, TypeAbbreviationDeclaration.TYPE_OR_UNION_CASE),
        ("ModuleAbbreviation", ElementType.MODULE_ABBREVIATION, ModuleAbbreviation.TYPE_REFERENCE),
      };

      lock (this)
      {
        bindingAndModuleDeclIndentingRulesParameters
          .Union(synExprIndentingRulesParameters)
          .Union(typeDeclarationIndentingRulesParameters)
          .ToList()
          .ForEach(DescribeSimpleIndentingRule);

        Describe<IndentingRule>()
          .Name("TryWith_WithClauseIndent")
          .Where(
            Parent().HasType(ElementType.TRY_WITH_EXPR),
            Node().HasRole(TryWithExpr.CLAUSE))
          .Switch(
            settings => settings.IndentOnTryWith,
            When(true).Return(IndentType.External),
            When(false).Return(IndentType.None))
          .Build();

        Describe<IndentingRule>()
          .Name("PrefixAppExprIndent")
          .Where(
            Parent().HasType(ElementType.PREFIX_APP_EXPR),
            Node()
              .HasRole(PrefixAppExpr.ARG_EXPR)
              .Satisfies((node, context) =>
                !(node is IComputationLikeExpr) ||
                !node.ContainsLineBreak(context.CodeFormatter)))
          .Return(IndentType.External)
          .Build();

        Describe<IndentingRule>()
          .Name("ElseExprIndent")
          .Where(
            Parent().In(ElementType.IF_THEN_ELSE_EXPR, ElementType.ELIF_EXPR),
            Node()
              .HasRole(IfThenElseExpr.ELSE_CLAUSE)
              .Satisfies(IndentElseExpr)
              .Or()
              .HasRole(ElifExpr.ELSE_CLAUSE)
              .Satisfies(IndentElseExpr))
          .Return(IndentType.External)
          .Build();

        Describe<IndentingRule>()
          .Name("MatchClauseExprIndent")
          .Where(
            Node().HasRole(MatchClause.EXPR),
            Parent()
              .HasType(ElementType.MATCH_CLAUSE)
              .Satisfies((node, context) =>
              {
                if (!(node is IMatchClause matchClause))
                  return false;

                var expr = matchClause.Expression;
                return !IsLastNodeOfItsType(node, context) ||
                       !AreAligned(matchClause, expr, context.CodeFormatter);
              }))
          .Return(IndentType.External)
          .Build();

        Describe<IndentingRule>()
          .Name("DoDeclIndent")
          .Where(
            Parent()
              .HasType(ElementType.DO)
              .Satisfies((node, context) => ((IDo) node).DoKeyword != null),
            Node().HasRole(Do.CHAMELEON_EXPR))
          .Return(IndentType.External)
          .Build();
      }
    }

    public override ProjectFileType MainProjectFileType => FSharpProjectFileType.Instance;

    private void DescribeSimpleIndentingRule((string name, CompositeNodeType parentType, short childRole) parameters)
    {
      Describe<IndentingRule>()
        .Name(parameters.name + "Indent")
        .Where(
          Parent().HasType(parameters.parentType),
          Node().HasRole(parameters.childRole))
        .Return(IndentType.External)
        .Build();
    }

    private static bool IndentElseExpr(ITreeNode elseExpr, CodeFormattingContext context) =>
      elseExpr.GetPreviousMeaningfulSibling().IsFirstOnLine(context.CodeFormatter) && !(elseExpr is IElifExpr);

    private static bool AreAligned(ITreeNode first, ITreeNode second, IWhitespaceChecker whitespaceChecker) =>
      first.CalcLineIndent(whitespaceChecker) == second.CalcLineIndent(whitespaceChecker);
  }
}
