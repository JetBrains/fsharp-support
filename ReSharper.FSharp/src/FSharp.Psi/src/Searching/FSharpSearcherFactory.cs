using System.Collections.Generic;
using System.Linq;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.Annotations;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.DeclaredElement.Compiled;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Util;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.Impl.Search;
using JetBrains.ReSharper.Psi.Impl.Search.SearchDomain;
using JetBrains.ReSharper.Psi.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Util;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Searching
{
  [PsiSharedComponent]
  public class FSharpSearcherFactory : DomainSpecificSearcherFactoryBase
  {
    private readonly SearchDomainFactory mySearchDomainFactory;
    private readonly CLRDeclaredElementSearcherFactory myClrSearchFactory;

    public FSharpSearcherFactory(SearchDomainFactory searchDomainFactory,
      CLRDeclaredElementSearcherFactory clrSearchFactory)
    {
      mySearchDomainFactory = searchDomainFactory;
      myClrSearchFactory = clrSearchFactory;
    }

    public override bool IsCompatibleWithLanguage(PsiLanguageType languageType) => languageType.Is<FSharpLanguage>();

    public override IDomainSpecificSearcher
      CreateReferenceSearcher(IDeclaredElementsSet elements, bool findCandidates) =>
      new FSharpReferenceSearcher(elements, findCandidates);

    public override IEnumerable<string> GetAllPossibleWordsInFile(IDeclaredElement element) =>
      FSharpNamesUtil.GetPossibleSourceNames(element);

    public override ISearchDomain GetDeclaredElementSearchDomain(IDeclaredElement declaredElement)
    {
      // todo: type abbreviations
      if (declaredElement is IFSharpLocalDeclaration localDeclaration)
        return mySearchDomainFactory.CreateSearchDomain(localDeclaration.GetSourceFile());

      if (declaredElement is IFSharpSymbolElement fsSymbolElement)
      {
        var fsSymbol = fsSymbolElement.Symbol;
        if (!(fsSymbol is FSharpActivePatternCase activePatternCase))
          return EmptySearchDomain.Instance;

        if (fsSymbolElement is ResolvedFSharpSymbolElement)
        {
          var patternEntity = activePatternCase.Group.DeclaringEntity?.Value;
          if (patternEntity != null)
          {
            var patternTypeElement = FSharpElementsUtil.GetDeclaredElement(patternEntity, fsSymbolElement.Module);
            return patternTypeElement != null
              ? myClrSearchFactory.GetDeclaredElementSearchDomain(patternTypeElement)
              : EmptySearchDomain.Instance;
          }
        }
      }

      if (declaredElement is TopActivePatternCase activePatternCaseElement)
      {
        var declaration = activePatternCaseElement.GetDeclaration();
        if (declaration?.GetContainingNode<IFSharpLocalDeclaration>() != null)
          return mySearchDomainFactory.CreateSearchDomain(declaration.GetSourceFile());

        var containingMemberDeclaration = declaration?.GetContainingNode<ITypeMemberDeclaration>();
        var containingMember = containingMemberDeclaration?.DeclaredElement;
        if (containingMember != null)
          return myClrSearchFactory.GetDeclaredElementSearchDomain(containingMember);
      }

      if (declaredElement is CompiledActivePatternCase compiledActivePatternCase)
        return myClrSearchFactory.GetDeclaredElementSearchDomain(compiledActivePatternCase.Origin);

      if (declaredElement is IFSharpAnonRecordFieldProperty fieldProperty)
        return mySearchDomainFactory.CreateSearchDomain(fieldProperty.Module);

      return EmptySearchDomain.Instance;
    }

    public override IEnumerable<RelatedDeclaredElement> GetRelatedDeclaredElements(IDeclaredElement element)
    {
      if (element is IUnionCase unionCase)
        return GetUnionCaseRelatedElements(unionCase);

      if (element is IGeneratedConstructorParameterOwner parameterOwner &&
          parameterOwner.GetGeneratedParameter() is { } parameter)
        return new[] {new RelatedDeclaredElement(parameter)};

      if (element is IFSharpProperty property)
        return GetPropertyRelatedElements(property);

      return EmptyList<RelatedDeclaredElement>.Instance;
    }

    private static IEnumerable<RelatedDeclaredElement> GetUnionCaseRelatedElements([NotNull] IUnionCase unionCase) =>
      unionCase.GetGeneratedMembers().Select(member => new RelatedDeclaredElement(member));

    private static IEnumerable<RelatedDeclaredElement> GetPropertyRelatedElements([NotNull] IFSharpProperty property) =>
      property.Getters.Concat(property.Setters).Select(member => new RelatedDeclaredElement(member));

    public override NavigateTargets GetNavigateToTargets(IDeclaredElement element)
    {
      if (element is ResolvedFSharpSymbolElement resolvedSymbolElement &&
          resolvedSymbolElement.Symbol is FSharpActivePatternCase activePatternCase)
      {
        var activePattern = activePatternCase.Group;

        var entityOption = activePattern.DeclaringEntity;
        var patternNameOption = activePattern.Name;
        if (entityOption == null || patternNameOption == null)
          return NavigateTargets.Empty;

        var typeElement = entityOption.Value.GetTypeElement(resolvedSymbolElement.Module);
        var pattern = typeElement.EnumerateMembers(patternNameOption.Value, true).FirstOrDefault() as IDeclaredElement;
        if (pattern is IFSharpTypeMember)
        {
          if (!(pattern.GetDeclarations().FirstOrDefault() is IFSharpDeclaration patternDecl))
            return NavigateTargets.Empty;

          var caseElement = patternDecl.GetActivePatternByIndex(activePatternCase.Index);
          if (caseElement != null)
            return new NavigateTargets(caseElement, false);
        }
        else if (pattern != null)
          return new NavigateTargets(pattern, false);
      }

      if (element is ISecondaryDeclaredElement {OriginElement: { } origin})
        return new NavigateTargets(origin, false);

      if (!(element is IFSharpTypeMember fsTypeMember) || fsTypeMember.CanNavigateTo)
        return NavigateTargets.Empty;

      return fsTypeMember.GetContainingType() is IDeclaredElement containingType
        ? new NavigateTargets(containingType, false)
        : NavigateTargets.Empty;
    }
  }
}
