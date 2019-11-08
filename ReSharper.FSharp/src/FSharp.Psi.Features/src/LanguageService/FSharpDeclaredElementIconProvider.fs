namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.LanguageService

open System.Runtime.InteropServices
open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Cache2.Parts
open JetBrains.ReSharper.Plugins.FSharp.Util
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Psi.ExtensionsAPI.Caches2
open JetBrains.ReSharper.Psi.Resources

[<DeclaredElementIconProvider>]
type FSharpDeclaredElementIconProvider() =
    let privateCase = compose PsiSymbolsThemedIcons.EnumMember.Id PsiSymbolsThemedIcons.ModifiersPrivate.Id
    let internalCase = compose PsiSymbolsThemedIcons.EnumMember.Id PsiSymbolsThemedIcons.ModifiersInternal.Id
    let mutableField = compose PsiSymbolsThemedIcons.Field.Id PsiSymbolsThemedIcons.ModifiersWrite.Id

    interface IDeclaredElementIconProvider with
        member x.GetImageId(declaredElement, _, [<Out>] canApplyExtensions) =
            canApplyExtensions <- true

            match declaredElement with
            | :? IModule -> FSharpIcons.FSharpModule.Id

            | :? IRepresentationAccessRightsOwner as accessRightsOwner ->
                canApplyExtensions <- false
                match accessRightsOwner.RepresentationAccessRights with
                | AccessRights.PRIVATE -> privateCase
                | AccessRights.INTERNAL -> internalCase
                | _ -> PsiSymbolsThemedIcons.EnumMember.Id

            | :? TypeElement as typeElement when
                    typeElement.PresentationLanguage.Is<FSharpLanguage>() && typeElement.IsUnion() ->
                PsiSymbolsThemedIcons.Enum.Id

            | :? IFSharpFieldProperty as fieldProp ->
                canApplyExtensions <- false
                if fieldProp.IsWritable then mutableField else PsiSymbolsThemedIcons.Field.Id

            | :? IActivePatternCase ->
                PsiSymbolsThemedIcons.EnumMember.Id

            | _ -> null
