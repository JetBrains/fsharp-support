﻿using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Psi;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
  internal partial class ObjectTypeDeclaration
  {
    private const string Interface = "Interface";
    private const string AbstractClass = "AbstractClass";
    private const string Class = "Class";
    private const string Sealed = "Sealed";
    private const string Struct = "Struct";

    public override string DeclaredName => Identifier.GetCompiledName(Attributes);
    public override string SourceName => Identifier.GetSourceName();
    public override TreeTextRange GetNameRange() => Identifier.GetNameRange();

    public FSharpPartKind TypePartKind
    {
      get
      {
        foreach (var attr in AttributesEnumerable)
        {
          var attrIds = attr.LongIdentifier.Identifiers;
          if (attrIds.IsEmpty)
            continue;

          switch (attrIds.Last()?.GetText().GetAttributeShortName())
          {
            case Interface:
              return FSharpPartKind.Interface;
            case AbstractClass:
            case Sealed:
            case Class:
              return FSharpPartKind.Class;
            case Struct:
              return FSharpPartKind.Struct;
          }
        }

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var member in TypeMembersEnumerable)
          if (!(member is IInterfaceInherit) && !(member is IAbstractSlot))
            return FSharpPartKind.Class;

        return FSharpPartKind.Interface;
      }
    }
  }
}