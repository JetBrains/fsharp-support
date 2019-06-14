﻿using System;
using System.Collections.Generic;
using System.Linq;
using FSharp.Compiler;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.Annotations;
using JetBrains.Diagnostics;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.Compiled;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.Caches;
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;
using JetBrains.Util.Logging;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Util
{
  /// <summary>
  /// Map FSharpSymbol elements (as seen by FSharp.Compiler.Service) to declared elements.
  /// </summary>
  public static class FSharpElementsUtil
  {
    [CanBeNull]
    public static ITypeElement GetTypeElement([NotNull] this FSharpEntity entity, [NotNull] IPsiModule psiModule)
    {
      if (((FSharpSymbol) entity).DeclarationLocation == null || entity.IsByRef || entity.IsProvidedAndErased)
        return null;

      if (!entity.IsFSharpAbbreviation)
      {
        var clrTypeName = entity.GetClrName();
        if (clrTypeName == null)
          return null;

        var typeElements = psiModule.GetSymbolScope().GetTypeElementsByCLRName(clrTypeName);
        if (typeElements.IsEmpty())
          return null;

        if (typeElements.Length == 1)
          return typeElements[0];

        var assemblySimpleName = entity.Assembly?.SimpleName;
        return assemblySimpleName != null
          ? typeElements.FirstOrDefault(typeElement => typeElement.Module.DisplayName == assemblySimpleName)
          : null;
      }

      var symbolScope = psiModule.GetSymbolScope();
      while (entity.IsFSharpAbbreviation)
      {
        // FCS returns Clr names for non-abbreviated types only, using fullname
        var typeElement = TryFindByNames(GetPossibleNames(entity), symbolScope);
        if (typeElement != null)
          return typeElement;

        var abbreviatedType = entity.AbbreviatedType;
        if (!abbreviatedType.HasTypeDefinition)
          return null;

        entity = entity.AbbreviatedType.TypeDefinition;
      }

      return entity.GetTypeElement(psiModule);
    }

    private static IEnumerable<string> GetPossibleNames([NotNull] FSharpEntity entity)
    {
      yield return entity.AccessPath + "." + entity.DisplayName;
      yield return entity.AccessPath + "." + entity.LogicalName;
      yield return ((FSharpSymbol) entity).FullName;
    }

    [CanBeNull]
    private static ITypeElement TryFindByNames([NotNull] IEnumerable<string> names, ISymbolScope symbolScope)
    {
      foreach (var name in names)
        if (symbolScope.GetElementsByQualifiedName(name).FirstOrDefault() is ITypeElement typeElement)
          return typeElement;
      return null;
    }

    [CanBeNull]
    private static INamespace GetDeclaredNamespace([NotNull] FSharpEntity entity, IPsiModule psiModule)
    {
      var name = entity.LogicalName;
      var containingNamespace = entity.Namespace?.Value;
      var fullName = containingNamespace != null ? containingNamespace + "." + name : name;
      var elements = psiModule.GetSymbolScope().GetElementsByQualifiedName(fullName);
      return elements.FirstOrDefault() as INamespace;
    }

    [CanBeNull]
    public static IDeclaredElement GetDeclaredElement([CanBeNull] this FSharpSymbol symbol,
      [NotNull] IPsiModule psiModule, [CanBeNull] IReferenceExpression referenceExpression = null)
    {
      if (symbol == null)
        return null;

      if (symbol is FSharpEntity entity)
      {
        if (entity.IsUnresolved)
          return null;

        if (entity.IsNamespace)
          return GetDeclaredNamespace(entity, psiModule);

        return GetTypeElement(entity, psiModule);
      }

      if (symbol is FSharpMemberOrFunctionOrValue mfv)
      {
        if (mfv.IsUnresolved) return null;

        if (!mfv.IsModuleValueOrMember)
        {
          var declaration = FindNode<IFSharpDeclaration>(mfv.DeclarationLocation, referenceExpression);
          if (declaration is IFSharpLocalDeclaration localDeclaration)
            return localDeclaration;

          return declaration is ISynPat
            ? declaration.DeclaredElement
            : null;
        }

        var memberEntity = mfv.IsModuleValueOrMember ? mfv.DeclaringEntity : null;
        if (memberEntity == null) return null;

        if (mfv.IsConstructor && mfv.DeclaringEntity?.Value is FSharpEntity ctorEntity && ctorEntity.IsProvided)
          return GetDeclaredElement(memberEntity.Value, psiModule, referenceExpression);

        var typeElement = GetTypeElement(memberEntity.Value, psiModule);
        if (typeElement == null) return null;

        var members = mfv.IsConstructor
          ? typeElement.Constructors.AsList<ITypeMember>()
          : typeElement.EnumerateMembers(mfv.GetMfvCompiledName(), true).AsList();

        switch (members.Count)
        {
          case 0:
            return null;
          case 1:
            return members[0];
        }

        var mfvXmlDocId = GetXmlDocId(mfv);
        return members.FirstOrDefault(member =>
          member is IFSharpMember fsMember && fsMember.Mfv?.XmlDocSig == mfvXmlDocId || 
          member.XMLDocId == mfvXmlDocId);
      }

      if (symbol is FSharpUnionCase unionCase)
      {
        if (unionCase.IsUnresolved) return null;

        var unionTypeElement = GetTypeElement(unionCase.ReturnType.TypeDefinition, psiModule);
        if (unionTypeElement == null) return null;

        var caseCompiledName = unionCase.CompiledName;
        var caseMember = unionTypeElement.GetMembers().FirstOrDefault(m =>
        {
          var shortName = m.ShortName;
          return shortName == caseCompiledName || shortName == "New" + caseCompiledName;
        });

        if (caseMember != null)
          return caseMember;

        var unionClrName = unionTypeElement.GetClrName();
        var caseDeclaredType = TypeFactory.CreateTypeByCLRName(unionClrName + "+" + caseCompiledName, psiModule);
        return caseDeclaredType.GetTypeElement();
      }

      if (symbol is FSharpField field)
        if (!field.IsUnresolved && field.DeclaringEntity?.Value is FSharpEntity fieldEntity)
          return GetTypeElement(fieldEntity, psiModule)?.EnumerateMembers(field.Name, true).FirstOrDefault();

      if (symbol is FSharpActivePatternCase patternCase)
        return GetActivePatternCaseElement(psiModule, referenceExpression, patternCase);

      if (symbol is FSharpGenericParameter parameter)
        return GetTypeParameter(parameter, referenceExpression);

      return null;
    }

    private static IDeclaredElement GetActivePatternCaseElement(IPsiModule psiModule,
      IReferenceExpression referenceExpression, FSharpActivePatternCase patternCase)
    {
      var pattern = patternCase.Group;
      var entity = pattern.DeclaringEntity?.Value;
      if (entity == null)
        return GetActivePatternCaseElement(patternCase, psiModule, referenceExpression);

      var typeElement = GetTypeElement(entity, psiModule);
      var patternName = pattern.Name?.Value;
      if (typeElement == null || !(patternName is var name) || name == null)
        return null;

      if (typeElement.Module.ContainingProjectModule is IProject)
        return GetActivePatternCaseElement(patternCase, psiModule, referenceExpression);

      var patternMfv = entity.MembersFunctionsAndValues.FirstOrDefault(mfv => mfv.LogicalName == patternName);
      var patternCompiledName = patternMfv?.CompiledName ?? patternCase.Name;
      if (typeElement.EnumerateMembers(patternCompiledName, true).FirstOrDefault() is IMethod method)
        return new CompiledActivePatternCase(method, patternCase.Name, patternCase.Index);

      return null;
    }

    [CanBeNull]
    private static ITypeParameter GetTypeParameter([NotNull] FSharpGenericParameter parameter,
      [CanBeNull] IReferenceExpression referenceOwnerToken = null)
    {
      var containingMemberDeclaration = referenceOwnerToken?.GetContainingNode<ITypeMemberDeclaration>();
      if (!(containingMemberDeclaration?.DeclaredElement is IFSharpTypeParametersOwner containingMember))
        return null;

      var parameterName = parameter.Name;
      var typeParameter = containingMember.AllTypeParameters.FirstOrDefault(param => param.ShortName == parameterName);
      return typeParameter;
    }

    public static IDeclaredElement GetActivePatternCaseElement([NotNull] FSharpActivePatternCase activePatternCase,
      [NotNull] IPsiModule psiModule, [CanBeNull] IReferenceExpression referenceOwnerToken)
    {
      var declaration = GetActivePatternDeclaration(activePatternCase, psiModule, referenceOwnerToken);
      return declaration?.GetActivePatternByIndex(activePatternCase.Index);
    }

    private static IFSharpDeclaration GetActivePatternDeclaration([NotNull] FSharpActivePatternCase activePatternCase,
      [NotNull] IPsiModule psiModule, IReferenceExpression referenceOwnerToken)
    {
      var activePattern = activePatternCase.Group;
      var declaringEntity = activePattern.DeclaringEntity?.Value;
      if (declaringEntity != null)
      {
        var patternName = activePattern.PatternName();
        var typeElement = GetTypeElement(declaringEntity, psiModule);
        var patternElement = typeElement.EnumerateMembers(patternName, true).FirstOrDefault();
        return patternElement?.GetDeclarations().FirstOrDefault() as IFSharpDeclaration;
      }

      var patternId = FindNode<IActivePatternId>(activePatternCase.DeclarationLocation, referenceOwnerToken);
      return patternId?.GetContainingNode<IFSharpDeclaration>();
    }

    [CanBeNull]
    private static string GetXmlDocId([NotNull] FSharpMemberOrFunctionOrValue mfv)
    {
      try
      {
        return mfv.XmlDocSig;
      }
      catch (Exception e)
      {
        Logger.LogMessage(LoggingLevel.WARN, "Could not get XmlDocId for {0}", mfv);
        Logger.LogExceptionSilently(e);
        return null;
      }
    }

    private static T FindNode<T>(Range.range range, [CanBeNull] ITreeNode node) where T : class, ITreeNode
    {
      var fsFile = node?.GetContainingFile() as IFSharpFile;
      var document = fsFile?.GetSourceFile()?.Document;
      if (document == null) return null;

      var idToken = fsFile.FindTokenAt(document.GetTreeEndOffset(range) - 1);
      return idToken?.GetContainingNode<T>(true);
    }
  }
}
