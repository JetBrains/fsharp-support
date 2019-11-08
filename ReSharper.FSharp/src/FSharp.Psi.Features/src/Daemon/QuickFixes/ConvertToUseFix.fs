namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes

open JetBrains.ReSharper.Feature.Services.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Plugins.FSharp.Psi.Util
open JetBrains.ReSharper.Psi.ExtensionsAPI.Tree
open JetBrains.ReSharper.Resources.Shell

type ConvertToUseFix(warning: ConvertToUseBindingWarning) =
    inherit QuickFixBase()

    let letExpr = warning.LetExpr

    override x.Text = "Convert to 'use' binding"

    override x.IsAvailable _ =
        isValid letExpr && isValid letExpr.LetOrUseToken

    override x.ExecutePsiTransaction(_, _) =
        use writeCookie = WriteLockCookie.Create(letExpr.IsPhysical())
        let tokenType = if letExpr :? ILetOrUseExpr then FSharpTokenType.USE else FSharpTokenType.USE_BANG
        ModificationUtil.ReplaceChild(letExpr.LetOrUseToken, tokenType.CreateLeafElement()) |> ignore
        null
