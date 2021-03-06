﻿using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
{
  public partial interface IFSharpTypeOldDeclaration : IFSharpTypeElementDeclaration
  {
    PartKind TypePartKind { get; }
    TreeNodeCollection<ITypeParameterDeclaration> TypeParameterDeclarations { get; }
  }
}
