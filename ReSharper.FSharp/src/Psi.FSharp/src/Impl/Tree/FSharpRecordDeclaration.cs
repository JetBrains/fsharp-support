﻿namespace JetBrains.ReSharper.Psi.FSharp.Impl.Tree
{
  internal partial class FSharpRecordDeclaration
  {
    public override string DeclaredName => Identifier.GetName();

    public override TreeTextRange GetNameRange()
    {
      return Identifier.GetNameRange();
    }
  }
}