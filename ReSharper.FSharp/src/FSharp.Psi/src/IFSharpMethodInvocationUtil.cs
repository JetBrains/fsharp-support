using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi
{
  public interface IFSharpMethodInvocationUtil
  {
    IParameter GetMatchingParameter(IFSharpExpression fsExpr);
    IParameter GetNamedArg(IFSharpExpression fsExpr);
    IFSharpArgumentsOwner GetArgumentsOwner(IFSharpExpression expr);
  }
}
