﻿using System;
using System.Collections.Generic;
using JetBrains.ReSharper.Plugins.FSharp.Checker;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;
using JetBrains.ReSharper.Plugins.FSharp.Psi.Util;
using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.ReSharper.Psi.ExtensionsAPI;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2;
using JetBrains.ReSharper.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2
{
  public class FSharpCacheDeclarationProcessor : TreeNodeVisitor
  {
    protected readonly ICacheBuilder Builder;
    private readonly FSharpCheckerService myCheckerService;

    public FSharpCacheDeclarationProcessor(ICacheBuilder builder, FSharpCheckerService checkerService)
    {
      Builder = builder;
      myCheckerService = checkerService;
    }

    private static FSharpFileKind GetFSharpFileKind(IFSharpFile file)
    {
      if (file is IFSharpImplFile) return FSharpFileKind.ImplFile;
      if (file is IFSharpSigFile) return FSharpFileKind.SigFile;
      throw new ArgumentOutOfRangeException();
    }

    public override void VisitFSharpFile(IFSharpFile fsFile)
    {
      var sourceFile = fsFile.GetSourceFile();
      if (sourceFile == null)
        return;

      var fileKind = GetFSharpFileKind(fsFile);
      var hasPairFile = myCheckerService.HasPairFile(sourceFile);

      Builder.CreateProjectFilePart(new FSharpProjectFilePart(sourceFile, fileKind, hasPairFile));

      foreach (var declaration in fsFile.DeclarationsEnumerable)
        declaration.Accept(this);
    }

    public void ProcessNamedModuleLikeDeclaration(INamedModuleLikeDeclaration decl, Part part)
    {
      var qualifiers = decl.LongIdentifier?.Qualifiers ?? TreeNodeCollection<ITokenNode>.Empty;
      foreach (var qualifier in qualifiers)
      {
        var qualifierName = Builder.Intern(qualifier.GetText().RemoveBackticks());
        Builder.StartPart(new QualifiedNamespacePart(qualifier.GetTreeStartOffset(), qualifierName));
      }

      Builder.StartPart(part);
      FinishModuleLikeDeclaration(decl);

      foreach (var _ in qualifiers)
        Builder.EndPart();
    }

    public override void VisitNamedNamespaceDeclaration(INamedNamespaceDeclaration decl) =>
      ProcessNamedModuleLikeDeclaration(decl, new DeclaredNamespacePart(decl));

    public override void VisitGlobalNamespaceDeclaration(IGlobalNamespaceDeclaration decl)
    {
      foreach (var memberDecl in decl.MembersEnumerable)
        memberDecl.Accept(this);
    }

    public override void VisitAnonModuleDeclaration(IAnonModuleDeclaration decl)
    {
      Builder.StartPart(new AnonModulePart(decl, Builder));
      FinishModuleLikeDeclaration(decl);
    }

    public override void VisitNamedModuleDeclaration(INamedModuleDeclaration decl) =>
      ProcessNamedModuleLikeDeclaration(decl, new NamedModulePart(decl, Builder));

    public override void VisitNestedModuleDeclaration(INestedModuleDeclaration decl)
    {
      Builder.StartPart(new NestedModulePart(decl, Builder));
      FinishModuleLikeDeclaration(decl);
    }

    private void FinishModuleLikeDeclaration(IModuleLikeDeclaration decl)
    {
      foreach (var memberDecl in decl.MembersEnumerable)
        memberDecl.Accept(this);
      Builder.EndPart();
    }

    public override void VisitMemberDeclaration(IMemberDeclaration decl)
    {
      Builder.AddDeclaredMemberName(decl.CompiledName);
    }

    public override void VisitLetModuleDecl(ILetModuleDecl letModuleDecl)
    {
      foreach (var binding in letModuleDecl.Bindings)
      {
        var headPattern = binding.HeadPattern;
        if (headPattern != null)
          ProcessTypeMembers(headPattern.Declarations);
      }
    }

    public override void VisitExceptionDeclaration(IExceptionDeclaration decl)
    {
      Builder.StartPart(new ExceptionPart(decl, Builder));
      ProcessTypeMembers(decl.MemberDeclarations);
      Builder.EndPart();
    }

    public override void VisitEnumDeclaration(IEnumDeclaration decl)
    {
      Builder.StartPart(new EnumPart(decl, Builder));
      foreach (var memberDecl in decl.EnumMembersEnumerable)
        Builder.AddDeclaredMemberName(memberDecl.DeclaredName);
      Builder.EndPart();
    }

    public override void VisitRecordDeclaration(IRecordDeclaration decl)
    {
      var recordPart =
        decl.HasAttribute(FSharpImplUtil.Struct)
          ? (Part) new StructRecordPart(decl, Builder)
          : new RecordPart(decl, Builder);

      Builder.StartPart(recordPart);
      ProcessTypeMembers(decl.MemberDeclarations);
      Builder.EndPart();
    }

    public override void VisitUnionDeclaration(IUnionDeclaration decl)
    {
      var unionCases = decl.UnionCases;

      var casesWithFieldsCount = 0;
      foreach (var unionCase in unionCases)
        if (unionCase is INestedTypeUnionCaseDeclaration)
          casesWithFieldsCount++;
      var hasPublicNestedTypes = casesWithFieldsCount > 0 && unionCases.Count > 1;

      var unionPart =
        decl.HasAttribute(FSharpImplUtil.Struct)
          ? (Part) new StructUnionPart(decl, Builder, false)
          : new UnionPart(decl, Builder, hasPublicNestedTypes);

      Builder.StartPart(unionPart);
      foreach (var unionCase in unionCases)
        unionCase.Accept(this);
      ProcessTypeMembers(decl.MemberDeclarations);
      Builder.EndPart();
    }

    public override void VisitNestedTypeUnionCaseDeclaration(INestedTypeUnionCaseDeclaration decl)
    {
      Builder.StartPart(new UnionCasePart(decl, Builder));
      ProcessTypeMembers(decl.MemberDeclarations);
      Builder.EndPart();
    }

    public override void VisitTypeAbbreviationDeclaration(ITypeAbbreviationDeclaration decl)
    {
      Builder.StartPart(new HiddenTypePart(decl, Builder));
      Builder.EndPart();
    }

    public override void VisitAbstractTypeDeclaration(IAbstractTypeDeclaration decl)
    {
      Builder.StartPart(new HiddenTypePart(decl, Builder));
      Builder.EndPart();
    }

    public override void VisitObjectModelTypeDeclaration(IObjectModelTypeDeclaration decl)
    {
      Builder.StartPart(CreateObjectTypePart(decl, false));
      ProcessTypeMembers(decl.MemberDeclarations);
      Builder.EndPart();
    }

    public override void VisitDelegateDeclaration(IDelegateDeclaration decl)
    {
      Builder.StartPart(new DelegatePart(decl, Builder));
      Builder.EndPart();
    }

    public override void VisitTypeExtensionDeclaration(ITypeExtensionDeclaration typeExtension)
    {
      if (typeExtension.IsTypePartDeclaration)
      {
        Builder.StartPart(CreateObjectTypePart(typeExtension, true));
        ProcessTypeMembers(typeExtension.MemberDeclarations);
        Builder.EndPart();
        return;
      }

      if (typeExtension.IsTypeExtensionAllowed)
        ProcessTypeMembers(typeExtension.MemberDeclarations);
    }

    private Part CreateObjectTypePart(IFSharpTypeDeclaration decl, bool isExtension)
    {
      switch (decl.TypePartKind)
      {
        case PartKind.Class:
          return isExtension ? (Part) new ClassExtensionPart(decl, Builder) : new ClassPart(decl, Builder);
        case PartKind.Struct:
          return isExtension ? (Part) new StructExtensionPart(decl, Builder) : new StructPart(decl, Builder);
        case PartKind.Interface:
          return new InterfacePart(decl, Builder);
        case PartKind.Enum:
          return new EnumPart(decl, Builder);
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void ProcessTypeMembers(IEnumerable<IDeclaration> declarations)
    {
      foreach (var declaration in declarations)
      {
        if (!(declaration is ITypeMemberDeclaration))
          continue;

        var declaredName = declaration.DeclaredName;
        if (declaredName != SharedImplUtil.MISSING_DECLARATION_NAME)
          Builder.AddDeclaredMemberName(declaredName);
      }
    }
  }
}
