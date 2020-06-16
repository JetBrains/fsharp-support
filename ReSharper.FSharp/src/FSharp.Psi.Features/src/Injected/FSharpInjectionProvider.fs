namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Injected

open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
open JetBrains.ReSharper.Psi.Caches
open JetBrains.ReSharper.Psi.Impl.Shared.InjectedPsi
open JetBrains.ReSharper.Psi.RegExp.ClrRegex
open JetBrains.ReSharper.Psi.RegExp.ClrRegex.Tree

[<SolutionComponent>]
type FSharpInjectionProvider
        (lifetime: Lifetime, solution: ISolution, persistentIndexManager: IPersistentIndexManager,
         providersViewer: InjectionNodeProvidersViewer, injectionTargetLanguage: FSharpLiteralInjectionTarget) =
    inherit LanguageInjectorProviderInLiteralsWithRangeMarkersBase<IClrRegularExpressionFile, FSharpToken, FSharpLiteralInjectionTarget>(lifetime, solution, persistentIndexManager, providersViewer, injectionTargetLanguage)

    override __.ProvidedInjectionID = "FsRegex"
    override __.SupportedOriginalLanguage = FSharpLanguage.Instance :> _
    override __.ProvidedLanguage = ClrRegexLanguage.Instance :> _
