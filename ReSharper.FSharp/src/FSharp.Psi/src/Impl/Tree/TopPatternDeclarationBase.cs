﻿using System;
using System.Collections.Generic;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
  internal partial class TopReferencePat
  {
    protected override string DeclaredElementName => NameIdentifier.GetCompiledName(Attributes);
    public override TreeTextRange GetNameRange() => NameIdentifier.GetNameRange();

    public override IFSharpIdentifierLikeNode NameIdentifier => ReferenceName?.Identifier;
  }

  internal partial class TopParametersOwnerPat
  {
    protected override string DeclaredElementName => NameIdentifier.GetCompiledName(Attributes);
    public override string SourceName => IsDeclaration ? base.SourceName : SharedImplUtil.MISSING_DECLARATION_NAME;
    public override TreeTextRange GetNameRange() => IsDeclaration ? base.GetNameRange() : TreeTextRange.InvalidRange;

    public override IFSharpIdentifierLikeNode NameIdentifier => Identifier;

    protected override IDeclaredElement CreateDeclaredElement() =>
      IsDeclaration ? base.CreateDeclaredElement() : null;
  }
  
  internal abstract class TopPatternDeclarationBase : FSharpProperTypeMemberDeclarationBase, IFunctionDeclaration
  {
    IFunction IFunctionDeclaration.DeclaredElement => base.DeclaredElement as IFunction;

    protected override IDeclaredElement CreateDeclaredElement()
    {
      var typeDeclaration = GetContainingNode<ITypeDeclaration>();
      if (typeDeclaration == null)
        return null;

      if (!(GetFSharpSymbol() is FSharpMemberOrFunctionOrValue mfv))
        return null;

      if (typeDeclaration is IFSharpTypeDeclaration)
      {
        if ((!mfv.CurriedParameterGroups.IsEmpty() || !mfv.GenericParameters.IsEmpty()) && !mfv.IsMutable)
          return new FSharpTypePrivateMethod(this, mfv);

        if (mfv.LiteralValue != null)
          return new FSharpLiteral(this);

        return new FSharpTypePrivateField(this); 
      }

      if (mfv.LiteralValue != null)
        return new FSharpLiteral(this);

      if (!mfv.IsValCompiledAsMethod())
        return new ModuleValue(this, mfv);

      return !mfv.IsInstanceMember && mfv.CompiledName.StartsWith("op_", StringComparison.Ordinal)
        ? (IDeclaredElement) new FSharpSignOperator<TopPatternDeclarationBase>(this, mfv)
        : new ModuleFunction(this, mfv);
    }

    public TreeNodeCollection<IAttribute> Attributes =>
      GetBinding()?.AllAttributes ??
      TreeNodeCollection<IAttribute>.Empty;

    [CanBeNull]
    protected IBinding GetBinding()
    {
      var node = Parent;
      while (node != null)
      {
        switch (node)
        {
          case IBinding binding:
            return binding;
          case ITopParametersOwnerPat _:
            return null;
          default:
            node = node.Parent;
            break;
        }
      }

      return null;
    }
  }

  internal abstract class SynPatternBase : FSharpCompositeElement
  {
    public virtual bool IsDeclaration => false;

    public virtual IEnumerable<IDeclaration> Declarations =>
      EmptyList<IDeclaration>.Instance;
    
    public TreeNodeCollection<IAttribute> Attributes => TreeNodeCollection<IAttribute>.Empty;
  }
}
