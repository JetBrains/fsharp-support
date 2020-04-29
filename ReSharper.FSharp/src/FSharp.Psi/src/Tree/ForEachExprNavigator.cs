using JetBrains.Annotations;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
{
  public partial class ForEachExprNavigator
  {
    [CanBeNull]
    public static IForEachExpr GetByInExpression([CanBeNull] ISynExpr param) =>
      param?.Parent is IForEachExpr forEachExpr && forEachExpr.InClause == param
        ? forEachExpr
        : null;
  }
}
