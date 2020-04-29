namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Intentions

open JetBrains.ReSharper.Feature.Services.ContextActions
open JetBrains.ReSharper.Feature.Services.Util
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree

[<ContextAction(Name = "ToRecursiveLetBindings", Group = "F#", Description = "To recursive")>]
type ToRecursiveLetBindingsAction(dataProvider: FSharpContextActionDataProvider) =
    inherit ContextActionBase()

    override x.Text = "To recursive"

    override x.IsAvailable _ =
        let letBindings = dataProvider.GetSelectedElement<ILetBindings>()
        if isNull letBindings || letBindings.IsRecursive then false else

        let letToken = letBindings.LetOrUseToken
        if isNull letToken then false else

        let ranges = DisjointedTreeTextRange.From(letToken)

        let bindings = letBindings.Bindings
        if bindings.Count <> 1 then false else

        match bindings.[0].HeadPattern.As<INamedPat>() with
        | null -> false
        | namedPat ->

        match namedPat.Identifier with
        | null -> false
        | identifier ->

        ranges.Then(identifier) |> ignore
        ranges.Contains(dataProvider.SelectedTreeRange)

    override x.ExecutePsiTransaction(_, _) =
        use cookie = FSharpRegistryUtil.AllowFormatterCookie.Create()
        let letBindings = dataProvider.GetSelectedElement<ILetBindings>()
        letBindings.SetIsRecursive(true)

        null
