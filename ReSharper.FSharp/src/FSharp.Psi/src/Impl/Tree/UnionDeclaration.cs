﻿using System.Collections.Generic;
using JetBrains.ReSharper.Plugins.FSharp.Common.Util;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
  internal partial class UnionDeclaration
  {
    protected override string DeclaredElementName => NameIdentifier.GetCompiledName(Attributes);
    public override IFSharpIdentifier NameIdentifier => (IFSharpIdentifier) Identifier;

    public override IReadOnlyList<ITypeMemberDeclaration> MemberDeclarations =>
      base.MemberDeclarations.Prepend(UnionCases).AsIReadOnlyList(); // todo: shit, rewrite it
    
    public override PartKind TypePartKind =>
      FSharpImplUtil.GetTypeKind(AttributesEnumerable, out var typeKind)
        ? typeKind
        : PartKind.Class;
  }
}
