﻿namespace JetBrains.ReSharper.Psi.FSharp.Impl.Tree
{
  internal partial class FSharpOtherSimpleTypeDeclaration
  {
    public override string DeclaredName => Identifier.GetName();

    public override TreeTextRange GetNameRange()
    {
      return Identifier.GetNameRange();
    }
  }
}