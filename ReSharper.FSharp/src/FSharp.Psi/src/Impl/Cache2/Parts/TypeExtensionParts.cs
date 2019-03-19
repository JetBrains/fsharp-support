using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts
{
  internal class ClassExtensionPart : FSharpTypeMembersOwnerTypePart, Class.IClassPart
  {
    public ClassExtensionPart([NotNull] IFSharpTypeDeclaration declaration, [NotNull] ICacheBuilder cacheBuilder)
      : base(declaration, cacheBuilder)
    {
    }

    public ClassExtensionPart(IReader reader) : base(reader)
    {
    }

    protected override byte SerializationTag =>
      (byte) FSharpPartKind.ClassExtension;

    public override TypeElement CreateTypeElement() =>
      new FSharpClass(this);
  }

  internal class StructExtensionPart : FSharpTypeMembersOwnerTypePart, Struct.IStructPart
  {
    public StructExtensionPart([NotNull] IFSharpTypeDeclaration declaration, [NotNull] ICacheBuilder cacheBuilder)
      : base(declaration, cacheBuilder)
    {
    }

    public StructExtensionPart(IReader reader) : base(reader)
    {
    }

    protected override byte SerializationTag =>
      (byte) FSharpPartKind.StructExtension;

    public override TypeElement CreateTypeElement() =>
      new FSharpStruct(this);

    public MemberPresenceFlag GetMembersPresenceFlag() =>
      MemberPresenceFlag.NONE;

    public bool HasHiddenInstanceFields => false;
    public bool IsReadonly => false;
    public bool IsByRefLike => false;
  }
}
