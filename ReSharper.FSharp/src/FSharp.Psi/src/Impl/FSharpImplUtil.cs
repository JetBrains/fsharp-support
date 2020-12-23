using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.Metadata.Reader.API;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.CompilerGenerated;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Util;
using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.Reflection2;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Naming;
using JetBrains.ReSharper.Psi.Parsing;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Resources.Shell;
using JetBrains.Util;
using JetBrains.Util.Extension;
using JetBrains.Util.Logging;
using Microsoft.FSharp.Core;
using PrettyNaming = FSharp.Compiler.PrettyNaming;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
{
  public static class FSharpImplUtil
  {
    public const string CompiledNameAttrName = "Microsoft.FSharp.Core.CompiledNameAttribute";
    public const string ModuleSuffixFlag = "CompilationRepresentationFlags.ModuleSuffix";
    public const string CompiledName = "CompiledName";
    public const string AttributeSuffix = "Attribute";
    public const string ModuleSuffix = "Module";
    public const string Interface = "Interface";
    public const string AbstractClass = "AbstractClass";
    public const string Class = "Class";
    public const string Sealed = "Sealed";
    public const string Struct = "Struct";
    public const string AutoOpen = "AutoOpen";

    [NotNull] public static string GetShortName([NotNull] this IAttribute attr) =>
      attr.ReferenceName?.ShortName.GetAttributeShortName() ?? SharedImplUtil.MISSING_DECLARATION_NAME;

    public static bool ShortNameEquals([NotNull] this IAttribute attr, [NotNull] string shortName) =>
      attr.GetShortName() == shortName;

    [CanBeNull]
    private static FSharpString GetStringConst([CanBeNull] IFSharpExpression expr)
    {
      switch (expr)
      {
        case IParenExpr parenExpr:
          return GetStringConst(parenExpr.InnerExpression);
        case IConstExpr constExpr:
          return constExpr.FirstChild as FSharpString;
      }
      return null;
    }

    private static bool GetCompiledNameValue(IAttribute attr, out string compiledName)
    {
      if (!attr.ShortNameEquals(CompiledName))
      {
        compiledName = null;
        return false;
      }

      // todo: proper expressions evaluation, e.g. "S1" + "S2"
      var stringArg = GetStringConst(attr.Expression);
      if (stringArg == null)
      {
        compiledName = null;
        return false;
      }

      compiledName =
        stringArg.GetText()
          .Substring(1, stringArg.GetText().Length - 2)
          .SubstringBeforeLast("`", StringComparison.Ordinal);
      return true;
    }

    private static bool IsModuleSuffixExpr([CanBeNull] IFSharpExpression expr)
    {
      switch (expr)
      {
        case ParenExpr parenExpr:
          return IsModuleSuffixExpr(parenExpr.InnerExpression);
        case IReferenceExpr referenceExpr:
          return referenceExpr.QualifiedName == ModuleSuffixFlag;
      }

      return false;
    }

    private static bool IsModuleSuffixAttribute([NotNull] this IAttribute attr) =>
      attr.ShortNameEquals("CompilationRepresentation") && IsModuleSuffixExpr(attr.Expression);

    public static string GetModuleCompiledName([CanBeNull] this IIdentifier identifier,
      TreeNodeCollection<IAttribute> attributes)
    {
      var hasModuleSuffix = false;
      string compiledName = null;

      foreach (var attr in attributes)
      {
        if (GetCompiledNameValue(attr, out var value))
          compiledName = value;

        if (!attr.IsModuleSuffixAttribute())
          continue;

        hasModuleSuffix = true;
        break;
      }

      var sourceName = identifier?.Name;
      return hasModuleSuffix && sourceName != null
        ? sourceName + "Module"
        : compiledName ?? sourceName ?? SharedImplUtil.MISSING_DECLARATION_NAME;
    }

    public static bool GetCompiledName(this TreeNodeCollection<IAttribute> attributes, out string name)
    {
      foreach (var attr in attributes)
        if (GetCompiledNameValue(attr, out var value))
        {
          name = value;
          return true;
        }

      name = default;
      return false;
    }

    [NotNull]
    public static string GetCompiledName([CanBeNull] this IIdentifier identifier,
      TreeNodeCollection<IAttribute> attributes) =>
      GetCompiledName(attributes, out var name)
        ? name
        : GetCompiledName(identifier);

    public static string GetCompiledName([CanBeNull] this IIdentifier identifier)
    {
      if (identifier == null)
        return SharedImplUtil.MISSING_DECLARATION_NAME;

      var name = identifier.Name;
      if (name == SharedImplUtil.MISSING_DECLARATION_NAME)
        return name;

      if (identifier is FSharpIdentifierToken token &&
          token.GetTokenType() is var tokenType && tokenType != FSharpTokenType.IDENTIFIER)
        return tokenType == FSharpTokenType.LPAREN_STAR_RPAREN
          ? "op_Multiply"
          : PrettyNaming.CompileOpName.Invoke(name);

      return name;
    }

    [NotNull]
    public static string GetSourceName([CanBeNull] this IIdentifier identifier) =>
      identifier?.Name ?? SharedImplUtil.MISSING_DECLARATION_NAME;

    [NotNull] 
    public static string GetSourceName([CanBeNull] this ITreeNode treeNode) =>
      GetSourceName(treeNode as IIdentifier);

    public static TreeTextRange GetNameRange([CanBeNull] this IFSharpIdentifierLikeNode identifier)
    {
      if (identifier == null)
        return TreeTextRange.InvalidRange;

      var nameRange = identifier.NameRange;
      var identifierToken = identifier.IdentifierToken;
      if (identifierToken == null)
        return nameRange;

      return identifierToken.GetText().IsEscapedWithBackticks()
        ? nameRange.TrimLeft(2).TrimRight(2)
        : nameRange;
    }

    public static TreeTextRange GetNameIdentifierRange([CanBeNull] this IFSharpIdentifierLikeNode identifier) =>
      identifier?.GetTreeTextRange() ?? TreeTextRange.InvalidRange;

    public static TreeTextRange GetMemberNameIdentifierRange([CanBeNull] this IFSharpIdentifierLikeNode identifier)
    {
      var range = identifier.GetNameIdentifierRange();
      return identifier?.GetTokenType() == FSharpTokenType.LPAREN_STAR_RPAREN
        ? range.TrimLeft(1).TrimRight(1)
        : range;
    }

    
    [NotNull]
    public static string MakeClrName([NotNull] this IFSharpTypeElementDeclaration declaration)
    {
      var clrName = new StringBuilder();

      var containingTypeDeclaration = declaration.GetContainingTypeDeclaration();
      if (containingTypeDeclaration != null)
      {
        clrName.Append(containingTypeDeclaration.CLRName).Append('+');
      }
      else
      {
        var namespaceDeclaration = declaration.GetContainingNamespaceDeclaration();
        if (namespaceDeclaration != null)
          clrName.Append(namespaceDeclaration.QualifiedName).Append('.');
      }

      clrName.Append(declaration.CompiledName);

      var typeDeclaration = declaration as IFSharpTypeOldDeclaration;
      if (typeDeclaration?.TypeParameters.Count > 0)
        clrName.Append("`" + typeDeclaration.TypeParameters.Count);

      return clrName.ToString();
    }

    [NotNull]
    public static string GetMfvCompiledName([NotNull] this FSharpMemberOrFunctionOrValue mfv)
    {
      try
      {
        var compiledNameAttr = mfv.Attributes.TryFindAttribute(CompiledNameAttrName);
        var compiledName = compiledNameAttr != null && !compiledNameAttr.Value.ConstructorArguments.IsEmpty()
          ? compiledNameAttr.Value.ConstructorArguments[0].Item2 as string
          : null;
        return compiledName ??
               (mfv.IsPropertyGetterMethod || mfv.IsPropertySetterMethod
                 ? mfv.DisplayName
                 : mfv.LogicalName);
      }
      catch (Exception e)
      {
        Logger.LogMessage(LoggingLevel.WARN, "Couldn't get CompiledName attribute value:");
        Logger.LogExceptionSilently(e);
      }

      return SharedImplUtil.MISSING_DECLARATION_NAME;
    }

    public static FSharpFileKind GetFSharpFileKind([CanBeNull] this IPsiSourceFile sourceFile)
    {
      var fileExtension = sourceFile?.GetLocation().ExtensionNoDot;
      if (fileExtension == "fsi" || fileExtension == "mli")
        return FSharpFileKind.SigFile;

      return FSharpFileKind.ImplFile;
    }

    [CanBeNull]
    internal static IDeclaredElement GetActivePatternByIndex(this IFSharpDeclaration declaration, int index)
    {
      if (!(declaration.NameIdentifier is ActivePatternId patternId))
        return null;

      var cases = patternId.Cases;
      if (index < 0 || index >= cases.Count)
        return null;

      var caseDeclaration = cases[index] as IActivePatternNamedCaseDeclaration;
      return caseDeclaration?.DeclaredElement;
    }

    public static TreeNodeCollection<IAttribute> GetAttributes([NotNull] this IDeclaration declaration)
    {
      switch (declaration)
      {
        case IFSharpTypeOldDeclaration typeDeclaration:
          return typeDeclaration.AllAttributes;
        case IMemberDeclaration memberDeclaration:
          return memberDeclaration.Attributes;
        case IFSharpPattern fsPattern:
          return fsPattern.Attributes;
        case IDeclaredModuleDeclaration moduleDeclaration:
          return moduleDeclaration.Attributes;
        default: return TreeNodeCollection<IAttribute>.Empty;
      }
    }

    public static string GetAttributeShortName([NotNull] this string attrName) =>
      attrName.TrimFromEnd(AttributeSuffix);

    public static bool IsCliMutableRecord([CanBeNull] this ITypeElement type)
    {
      if (!(type is TypeElement typeElement))
        return false;

      // todo: climutable attr can be on anon part (`type R`)
      foreach (var part in typeElement.EnumerateParts())
        if (part is IRecordPart recordPart && recordPart.CliMutable)
          return true;

      return false;
    }

    [CanBeNull]
    public static TPart GetPart<TPart>([CanBeNull] this ITypeElement type)
      where TPart : class, ClassLikeTypeElement.IClassLikePart
    {
      // todo: check signature first if one is present

      if (!(type is TypeElement typeElement))
        return null;

      foreach (var part in typeElement.EnumerateParts())
        if (part is TPart expectedPart)
          return expectedPart;

      return null;
    }

    public static bool IsException(this ITypeElement typeElement) =>
      typeElement switch
      {
        IFSharpTypeElement fsTypeElement => fsTypeElement.GetPart<IExceptionPart>() != null,
        ICompiledElement compiled when compiled.IsFromFSharpAssembly() => compiled.IsCompiledException(),
        _ => false
      };
    
    public static bool IsUnion([NotNull] this ITypeElement typeElement) =>
      typeElement switch
      {
        IFSharpTypeElement fsTypeElement => fsTypeElement.GetPart<IUnionPart>() != null,
        ICompiledElement compiled when compiled.IsFromFSharpAssembly() => compiled.IsCompiledUnion(),
        _ => false
      };

    public static bool IsUnionCase([NotNull] this ITypeElement typeElement) => 
      typeElement switch
      {
        IFSharpTypeElement fsTypeElement => fsTypeElement.GetPart<UnionCasePart>() != null,
        ICompiledElement compiled when compiled.IsFromFSharpAssembly() => compiled.IsCompiledUnionCase(),
        _ => false
      };

    [NotNull]
    public static IList<IUnionCase> GetSourceUnionCases([CanBeNull] this ITypeElement type) =>
      GetPart<IUnionPart>(type)?.Cases ?? EmptyList<IUnionCase>.Instance;

    [CanBeNull]
    public static FSharpUnionTagsClass GetUnionTagsClass([CanBeNull] this ITypeElement type) =>
      GetPart<IUnionPart>(type) is UnionPartBase unionPart && !unionPart.IsSingleCase
        ? new FSharpUnionTagsClass(unionPart.TypeElement)
        : null;

    public static IParametersOwner GetGeneratedConstructor(this ITypeElement type)
    {
      if (!(type is TypeElement typeElement))
        return null;

      foreach (var part in typeElement.EnumerateParts())
        if (part is IGeneratedConstructorOwner constructorOwnerPart)
          return constructorOwnerPart.GetConstructor();

      return null;
    }

    public static string GetSourceName([NotNull] this TypeElement typeElement)
    {
      foreach (var part in typeElement.EnumerateParts())
        if (part is IFSharpTypePart fsTypePart)
          return fsTypePart.SourceName;

      return typeElement.ShortName;
    }

    public static bool HasModuleSuffix([NotNull] this string shortName) =>
      shortName.EndsWith(ModuleSuffix, StringComparison.Ordinal);

    public static ITypeElement TryGetAssociatedType([NotNull] this CompiledTypeElement moduleTypeElement, string sourceName)
    {
      Assertion.Assert(moduleTypeElement.IsCompiledModule(), "moduleTypeElement.IsCompiledModule()");

      bool IsAssociatedType(ITypeElement t) =>
        !t.Equals(moduleTypeElement) && t.TypeParameters.Count == 0 && 
        !t.IsCompiledModule() && t.GetSourceName() == sourceName;

      var containingType = moduleTypeElement.GetContainingType();
      if (containingType != null)
        return containingType.NestedTypes.FirstOrDefault(IsAssociatedType);

      var ns = moduleTypeElement.GetContainingNamespace();
      var symbolScope = moduleTypeElement.Module.GetModuleOnlySymbolScope();
      return ns.GetNestedTypeElements(symbolScope).FirstOrDefault(IsAssociatedType);
    }

    public static string GetSourceName([NotNull] this CompiledTypeElement typeElement)
    {
      if (typeElement.GetAttributeFirstArgValue(FSharpPredefinedType.SourceNameAttrTypeName) is string sourceName &&
          sourceName != SharedImplUtil.MISSING_DECLARATION_NAME)
        return sourceName;

      var shortName = typeElement.ShortName;
      if (shortName.HasModuleSuffix() && typeElement.IsCompiledModule())
      {
        var shortNameWithoutSuffix = shortName.SubstringBeforeLast(ModuleSuffix);
        var flags = typeElement.GetAttributeFirstArgValue(FSharpPredefinedType.CompilationRepresentationAttrTypeName);
        if (flags != null && (CompilationRepresentationFlags) flags == CompilationRepresentationFlags.ModuleSuffix)
          return shortNameWithoutSuffix;

        if (typeElement.TryGetAssociatedType(shortNameWithoutSuffix) != null)
          return shortNameWithoutSuffix;
      }

      return shortName;
    }

    public static string GetSourceName([NotNull] this IDeclaredElement declaredElement) =>
      declaredElement switch
      {
        INamespace ns => ns.IsRootNamespace ? "global" : ns.ShortName,
        IFSharpDeclaredElement fsElement => fsElement.SourceName,
        CompiledTypeElement compiledTypeElement => GetSourceName(compiledTypeElement),
        _ => declaredElement.ShortName
      };

    public static AccessRights GetFSharpRepresentationAccessRights([CanBeNull] this ITypeElement type)
    {
      if (!(type is TypeElement typeElement))
        return AccessRights.PUBLIC;

      foreach (var part in typeElement.EnumerateParts())
        if (part is IRepresentationAccessRightsOwner accessRightsOwner)
          return accessRightsOwner.RepresentationAccessRights;
      return AccessRights.PUBLIC;
    }

    public static AccessRights GetRepresentationAccessRights([CanBeNull] this ITypeElement type)
    {
      if (!(type is TypeElement typeElement))
        return AccessRights.PUBLIC;

      foreach (var part in typeElement.EnumerateParts())
        switch (part)
        {
          case UnionCasePart casePart:
            if (casePart.Parent is IUnionPart parent && parent.RepresentationAccessRights != AccessRights.PUBLIC)
              return AccessRights.INTERNAL;
            break;

          case IRepresentationAccessRightsOwner accessRightsOwner:
            if (accessRightsOwner.RepresentationAccessRights != AccessRights.PUBLIC)
              return AccessRights.INTERNAL;
            break;
        }

      return AccessRights.PUBLIC;
    }

    // todo: hidden by signature in fsi
    public static AccessRights GetRepresentationAccessRights([NotNull] this IFSharpTypeDeclaration declaration) =>
      declaration.TypeRepresentation is ISimpleTypeRepresentation repr
        ? ModifiersUtil.GetAccessRights(repr.AccessModifier)
        : AccessRights.PUBLIC;

    public static PartKind GetSimpleTypeKindFromAttributes(this IFSharpTypeDeclaration decl) => 
      GetTypeKind(decl.AllAttributes, out var kind) ? kind : PartKind.Class; // todo struct or class only

    public static bool GetTypeKind(TreeNodeCollection<IAttribute> attributes, out PartKind fSharpPartKind)
    {
      foreach (var attr in attributes)
        switch (attr.ReferenceName?.ShortName.DropAttributeSuffix())
        {
          case Interface:
          {
            fSharpPartKind = PartKind.Interface;
            return true;
          }

          case AbstractClass:
          case Sealed:
          case Class:
          {
            fSharpPartKind = PartKind.Class;
            return true;
          }

          case Struct:
          {
            fSharpPartKind = PartKind.Struct;
            return true;
          }
        }

      fSharpPartKind = default;
      return false;
    }

    public static PartKind GetTypeKind([NotNull] this IFSharpTypeDeclaration declaration)
    {
      if (GetTypeKind(declaration.AllAttributes, out var typeKind))
        return typeKind;

      foreach (var member in declaration.TypeMembersEnumerable)
        if (!(member is IInterfaceInherit) && !(member is IAbstractMemberDeclaration))
          return PartKind.Class;

      return PartKind.Interface;
    }

    [NotNull]
    public static TypeAugmentation GetTypeAugmentationInfo([NotNull] ITypeExtensionDeclaration declaration)
    {
      var extensionNameInfo = new NameAndParametersCount(declaration.SourceName, declaration.TypeParameters.Count);
      var declaredTypeNames = new Dictionary<NameAndParametersCount, TypeAugmentation>();

      void RecordName(IFSharpTypeOldDeclaration typeDeclaration)
      {
        var sourceName = typeDeclaration.SourceName;
        if (sourceName == SharedImplUtil.MISSING_DECLARATION_NAME)
          return;

        // todo: should check source name only
        var compiledName = typeDeclaration.CompiledName;
        if (compiledName == SharedImplUtil.MISSING_DECLARATION_NAME)
          return;

        var parametersCount = typeDeclaration.TypeParameters.Count;
        var augmentationInfo =
          TypeAugmentation.NewTypePart(compiledName, parametersCount, typeDeclaration.TypePartKind);

        var nameInfo = new NameAndParametersCount(sourceName, parametersCount);
        declaredTypeNames[nameInfo] = augmentationInfo;
      }

      var ownTypeDeclarationGroup = TypeDeclarationGroupNavigator.GetByTypeDeclaration(declaration);
      var moduleDeclaration = ModuleLikeDeclarationNavigator.GetByMember(ownTypeDeclarationGroup).NotNull();

      foreach (var member in moduleDeclaration.MembersEnumerable)
      {
        if (member is IExceptionDeclaration exceptionDeclaration)
        {
          RecordName(exceptionDeclaration);
          continue;
        }

        if (!(member is ITypeDeclarationGroup declarationGroup))
          continue;

        foreach (var typeDeclaration in declarationGroup.TypeDeclarations)
          if (!(typeDeclaration is ITypeExtensionDeclaration))
            RecordName(typeDeclaration);
      }

      return declaredTypeNames.TryGetValue(extensionNameInfo, out var typeAugmentation)
        ? typeAugmentation
        : TypeAugmentation.Extension;
    }

    public static string DropAttributeSuffix([NotNull] this string attrName) =>
      attrName.SubstringBeforeLast(AttributeSuffix, StringComparison.Ordinal);

    public static bool HasAttribute(this TreeNodeCollection<IAttribute> attributes, [NotNull] string shortName)
    {
      foreach (var attr in attributes)
        if (attr.ShortNameEquals(shortName))
          return true;

      return false;
    }
    
    public static bool HasAttribute([NotNull] this IFSharpTypeOldDeclaration typeDeclaration, [NotNull] string shortName) =>
      HasAttribute(typeDeclaration.AllAttributes, shortName);

    public static void ReplaceIdentifier([CanBeNull] this IFSharpIdentifierLikeNode fsIdentifier, string name)
    {
      var token = fsIdentifier?.IdentifierToken;
      if (token == null)
        return;

      name = NamingManager.GetNamingLanguageService(fsIdentifier.Language).MangleNameIfNecessary(name);
      using (WriteLockCookie.Create(fsIdentifier.IsPhysical()))
        LowLevelModificationUtil.ReplaceChildRange(token, token, new FSharpIdentifierToken(name));
    }

    public static void AddTokenAfter([NotNull] this ITreeNode anchor, [NotNull] TokenNodeType tokenType)
    {
      using var _ = WriteLockCookie.Create(anchor.NotNull().IsPhysical());
      ModificationUtil.AddChildAfter(anchor, tokenType.CreateLeafElement());
    }

    public static void AddTokenBefore([NotNull] ITreeNode anchor, [NotNull] TokenNodeType tokenType)
    {
      using var _ = WriteLockCookie.Create(anchor.NotNull().IsPhysical());
      var space = ModificationUtil.AddChildBefore(anchor, new Whitespace());
      ModificationUtil.AddChildBefore(space, tokenType.CreateLeafElement());
    }
    
    public static IList<ITypeElement> ToTypeElements(this IList<IClrTypeName> names, IPsiModule psiModule)
    {
      var result = new List<ITypeElement>(names.Count);
      foreach (var clrTypeName in names)
      {
        var typeElement = clrTypeName?.CreateTypeByClrName(psiModule).GetTypeElement();
        if (typeElement != null)
          result.Add(typeElement);
      }

      return result;
    }

    [CanBeNull]
    public static IDeclaredElement GetModuleToUpdateName([NotNull] this IFSharpTypeElement fsTypeElement,
      [CanBeNull] string newName)
    {
      if (!(fsTypeElement is TypeElement typeElement))
        return null;

      var typeSourceName = fsTypeElement.SourceName;
      foreach (var part in typeElement.EnumerateParts())
      {
        foreach (var child in part.Parent.NotNull("part.Parent != null").Children())
        {
          if (!(child is IModulePart && child is TypePart typePart))
            continue;

          if (!(typePart.TypeElement is IFSharpTypeElement otherTypeElement))
            continue;

          var sourceName = otherTypeElement.SourceName;
          if (sourceName == typeSourceName || sourceName == newName)
            return otherTypeElement;
        }
      }

      return null;
    }

    public static bool HasAttribute([NotNull] this TypeElement typeElement, string attrShortName)
    {
      foreach (var part in typeElement.EnumerateParts())
        if (part.AttributeClassNames.Contains(attrShortName))
          return true;
      return false;
    }

    public static bool HasAutoOpenAttribute([NotNull] this ITypeElement typeElement) =>
      typeElement switch
      {
        FSharpModule fsModule => HasAttribute(fsModule, AutoOpen),
        IFSharpTypeElement _ => false,
        _ => typeElement.HasAttributeInstance(FSharpPredefinedType.AutoOpenAttrTypeName, false)
      };

    public static bool IsRecord([NotNull] this ITypeElement typeElement) =>
      typeElement switch
      {
        IFSharpTypeElement fsTypeElement => (fsTypeElement.GetPart<IRecordPart>() != null),
        ICompiledElement compiledElement when compiledElement.Module.IsFSharpAssembly() =>
        (compiledElement.GetCompilationMappingFlag() == SourceConstructFlags.RecordType),
        _ => false
      };

    public static ICollection<string> GetRecordFieldNames([NotNull] this ITypeElement typeElement)
    {
      switch (typeElement)
      {
        case IFSharpTypeElement fsTypeElement:
          return fsTypeElement.GetPart<IRecordPart>()?.Fields.Select(f => f.ShortName).AsCollection() ??
                 EmptyList<string>.InstanceList;

        case ICompiledElement _:
          return typeElement.Properties.Where(p => p.IsCompiledFSharpField()).Select(p => p.ShortName).AsCollection();

        default:
          return EmptyArray<string>.Instance;
      }
    }

    public static bool IsModule(this ITypeElement typeElement) =>
      typeElement is IFSharpModule ||
      typeElement is ICompiledElement compiledElement && compiledElement.IsCompiledModule();

    public static ModuleMembersAccessKind GetAccessType([NotNull] this ITypeElement typeElement)
    {
      Assertion.Assert(typeElement.IsModule(), "typeElement.IsModule()");
      return typeElement switch
      {
        IFSharpModule module => module.AccessKind,
        ICompiledElement _ =>
          typeElement.HasRequireQualifiedAccessAttribute()
            ? ModuleMembersAccessKind.RequiresQualifiedAccess
            : typeElement.HasAutoOpenAttribute() ? ModuleMembersAccessKind.AutoOpen : ModuleMembersAccessKind.Normal,
        _ => throw new InvalidOperationException()
      };
    }

    public static bool RequiresQualifiedAccess([NotNull] this ITypeElement typeElement) => 
      typeElement.GetAccessType() == ModuleMembersAccessKind.RequiresQualifiedAccess;

    [CanBeNull]
    public static IFSharpExpression IgnoreParentParens([CanBeNull] this IFSharpExpression fsExpr)
    {
      if (fsExpr == null) return null;

      while (fsExpr.Parent is IParenExpr parenExpr)
        fsExpr = parenExpr;
      return fsExpr;
    }

    public static ITreeNode IgnoreParentChameleonExpr([NotNull] this ITreeNode treeNode) =>
      treeNode.Parent is IChameleonExpression parenExpr
        ? parenExpr.Parent
        : treeNode.Parent;

    [CanBeNull]
    public static IFSharpExpression IgnoreInnerParens([CanBeNull] this IFSharpExpression fsExpr)
    {
      if (fsExpr == null)
        return null;

      while (fsExpr is IParenExpr parenExpr && parenExpr.InnerExpression != null)
        fsExpr = parenExpr.InnerExpression;
      return fsExpr;
    }
    
    public static IFSharpPattern IgnoreParentParens([CanBeNull] this IFSharpPattern fsPattern)
    {
      if (fsPattern == null)
        return null;

      while (fsPattern.Parent is IParenPat parenPat)
        fsPattern = parenPat;
      return fsPattern;
    }

    public static IFSharpPattern IgnoreInnerParens([CanBeNull] this IFSharpPattern fsPattern)
    {
      if (fsPattern == null)
        return null;

      while (fsPattern is IParenPat parenPat && parenPat.Pattern != null)
        fsPattern = parenPat.Pattern;
      return fsPattern;
    }
    
    public static IEnumerable<IFSharpPattern> GetInnerMatchStatementPatterns([CanBeNull] this IFSharpPattern fsPattern)
    {
      if (fsPattern == null) 
        yield break;
      
      while (fsPattern is IParenPat parenPat && parenPat.Pattern != null)
        fsPattern = parenPat.Pattern;

      if (fsPattern is ITuplePat tuplePat)
      {
        foreach (var pattern in tuplePat.Patterns)
        {
          yield return pattern;
        }
      }
      else
      {
        yield return fsPattern;
      }
    }
    
    [NotNull]
    public static IFSharpReferenceOwner SetName([NotNull] this IFSharpReferenceOwner referenceOwner, 
      [NotNull] string name)
    {
      if (referenceOwner.FSharpIdentifier?.IdentifierToken is { } id)
        LowLevelModificationUtil.ReplaceChildRange(id, id, new FSharpIdentifierToken(name));

      return referenceOwner;
    }

    [NotNull]
    public static string GetQualifiedName([CanBeNull] IReferenceName qualifier,
      [CanBeNull] IFSharpIdentifier identifier)
    {
      if (qualifier == null && identifier == null)
        return SharedImplUtil.MISSING_DECLARATION_NAME;

      if (qualifier == null)
        return identifier.Name;

      return identifier != null
        ? qualifier.QualifiedName + "." + identifier.Name
        : qualifier.QualifiedName;
    }

    public static IList<string> GetNames([CanBeNull] this IReferenceExpr referenceExpr)
    {
      var result = new List<string>();
      while (referenceExpr != null)
      {
        var shortName = referenceExpr.ShortName;
        if (shortName.IsEmpty() || shortName == SharedImplUtil.MISSING_DECLARATION_NAME)
          break;

        result.Insert(0, shortName);
        referenceExpr = referenceExpr.Qualifier as IReferenceExpr;
      }

      return result;
    }

    
    public static ModuleMembersAccessKind GetAccessType([NotNull] this IDeclaredModuleDeclaration moduleDeclaration)
    {
      var autoOpen = false;

      foreach (var attr in moduleDeclaration.AttributesEnumerable)
        switch (attr.ReferenceName?.ShortName.DropAttributeSuffix())
        {
          case "AutoOpen":
            autoOpen = true;
            break;
          case "RequireQualifiedAccess":
            return ModuleMembersAccessKind.RequiresQualifiedAccess;
        }

      return autoOpen
        ? ModuleMembersAccessKind.AutoOpen
        : ModuleMembersAccessKind.Normal;
    }

    public static IList<ITypeParameter> GetAllTypeParametersReversed(this ITypeElement typeElement) =>
      typeElement.GetAllTypeParameters().ResultingList().Reverse();

    [CanBeNull]
    public static IDeclaredElement TryCreateOperator<TDeclaration>([NotNull] this TDeclaration decl)
      where TDeclaration : FSharpDeclarationBase, IModifiersOwnerDeclaration, ITypeMemberDeclaration
    {
      var name = decl.DeclaredName;
      if (!name.StartsWith("op_", StringComparison.Ordinal))
        return null;

      switch (name)
      {
        case StandardOperatorNames.Explicit:
          return new FSharpConversionOperator<TDeclaration>(decl, true);
        case StandardOperatorNames.Implicit:
          return new FSharpConversionOperator<TDeclaration>(decl, false);
        default:
          return new FSharpSignOperator<TDeclaration>(decl);
      }
    }

    public static IDeclaredElement GetOrCreateDeclaredElement<T>(this T decl, Func<T, IDeclaredElement> factory)
      where T : ICachedTypeMemberDeclaration
    {
      decl.AssertIsValid("Asking declared element from invalid declaration");
      var cache = decl.GetPsiServices().Caches.SourceDeclaredElementsCache;
      // todo: calc types on demand in members (move cookie to FSharpTypesUtil)
      using (CompilationContextCookie.GetOrCreate(decl.GetPsiModule().GetContextFromModule()))
        return cache.GetOrCreateDeclaredElement(decl, factory);
    }
  }
}
