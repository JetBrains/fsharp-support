﻿using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts
{
  internal class StructPart : FSharpTypeMembersOwnerTypePart, Struct.IStructPart
  {
    public StructPart([NotNull] IFSharpTypeDeclaration declaration, [NotNull] ICacheBuilder cacheBuilder)
      : base(declaration, cacheBuilder)
    {
    }

    public StructPart(IReader reader) : base(reader)
    {
    }

    public override TypeElement CreateTypeElement()
    {
      return new FSharpStruct(this);
    }

    public MemberPresenceFlag GetMembersPresenceFlag()
    {
      return MemberPresenceFlag.SIGN_OP;
    }

    public bool HasHiddenInstanceFields => false; // todo: check this
    public bool IsReadonly => false;
    public bool IsByRefLike => false;
    protected override byte SerializationTag => (byte) FSharpPartKind.Struct;
  }

  public class FSharpStruct : Struct
  {
    public FSharpStruct([NotNull] IStructPart part) : base(part)
    {
    }

    protected override MemberDecoration Modifiers => myParts.GetModifiers();
  }
}