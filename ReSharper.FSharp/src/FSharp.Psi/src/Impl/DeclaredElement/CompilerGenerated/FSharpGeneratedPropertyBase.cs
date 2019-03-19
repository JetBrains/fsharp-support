﻿using System.Collections.Generic;
using JetBrains.Annotations;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Impl.Special;
using JetBrains.ReSharper.Psi.Resolve;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.CompilerGenerated
{
  public abstract class FSharpGeneratedPropertyFromTypeBase : FSharpGeneratedPropertyBase
  {
    protected FSharpGeneratedPropertyFromTypeBase(ITypeElement typeElement) =>
      ContainingType = typeElement;

    public override ITypeElement ContainingType { get; }
  }

  public abstract class FSharpGeneratedPropertyBase : FSharpGeneratedMemberBase, IProperty
  {
    protected override IClrDeclaredElement ContainingElement => ContainingType;
    public override ITypeElement GetContainingType() => ContainingType;
    public override ITypeMember GetContainingTypeMember() => (ITypeMember) ContainingType;

    [NotNull] public abstract ITypeElement ContainingType { get; }

    public abstract IType Type { get; }

    public IType ReturnType => Type;
    public ReferenceKind ReturnKind => ReferenceKind.VALUE;

    public bool IsReadable => true;

    public IAccessor Getter =>
      new ImplicitAccessor(this, AccessorKind.GETTER);

    public override DeclaredElementType GetElementType() =>
      CLRDeclaredElementType.PROPERTY;

    public InvocableSignature GetSignature(ISubstitution substitution) =>
      new InvocableSignature(this, substitution);

    public IEnumerable<IParametersOwnerDeclaration> GetParametersOwnerDeclarations() =>
      EmptyList<IParametersOwnerDeclaration>.Instance;

    public string GetDefaultPropertyMetadataName() => ShortName;
    public IList<IParameter> Parameters => EmptyList<IParameter>.Instance;

    public IAccessor Setter => null;
    public bool IsWritable => false;
    public bool IsAuto => false;
    public bool IsDefault => false;
  }
}
