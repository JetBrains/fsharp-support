﻿using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
  internal partial class RecordFieldDeclaration
  {
    protected override string DeclaredElementName => NameIdentifier.GetSourceName();
    public override IFSharpIdentifierLikeNode NameIdentifier => (IFSharpIdentifierLikeNode) Identifier;

    protected override IDeclaredElement CreateDeclaredElement() =>
      new FSharpRecordField(this);

    public bool IsMutable => MutableKeyword != null;

    public void SetIsMutable(bool value)
    {
      if (value == IsMutable)
        return;

      if (!value)
        throw new System.NotImplementedException();

      var identifier = Identifier;
      if (identifier != null)
        FSharpImplUtil.AddTokenBefore(identifier, FSharpTokenType.MUTABLE);
    }
  }
}
