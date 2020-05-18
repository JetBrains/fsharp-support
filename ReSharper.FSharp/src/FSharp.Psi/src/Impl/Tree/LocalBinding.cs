using System.Linq;
using System.Xml;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
  internal partial class LocalBinding
  {
    public TreeNodeCollection<IAttribute> AllAttributes => Attributes;
    public bool IsMutable => MutableKeyword != null;

    public void SetIsMutable(bool value)
    {
      if (!value)
        throw new System.NotImplementedException();

      var headPat = HeadPattern;
      if (headPat != null)
        FSharpImplUtil.AddTokenBefore(headPat, FSharpTokenType.MUTABLE);
    }

    /// A workaround for getting a declared element for binding in features like Find Usages results and
    /// file member navigation where we're looking for containing type member.
    [CanBeNull]
    private ITypeMemberDeclaration FirstDeclaration =>
      HeadPattern is ITypeMemberDeclaration headPattern
        ? headPattern
        : HeadPattern?.Declarations.FirstOrDefault() as ITypeMemberDeclaration;
    
    public XmlNode GetXMLDoc(bool inherit) => null;

    public void SetName(string name) {}

    public TreeTextRange GetNameRange()
    {
      var headPattern = HeadPattern;
      if (headPattern == null)
        return TreeTextRange.InvalidRange;

      return headPattern.Declarations.SingleItem()?.GetNameRange() ?? headPattern.GetTreeTextRange();
    }

    public bool IsSynthetic() => false;
    public IDeclaredElement DeclaredElement => FirstDeclaration?.DeclaredElement as IFunction;

    IFunction IFunctionDeclaration.DeclaredElement => FirstDeclaration?.DeclaredElement as IFunction;

    public string DeclaredName => SharedImplUtil.MISSING_DECLARATION_NAME;
  }
}
