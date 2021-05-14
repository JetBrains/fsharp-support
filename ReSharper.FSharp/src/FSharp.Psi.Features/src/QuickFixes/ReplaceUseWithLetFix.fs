namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes

open JetBrains.Diagnostics
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Parsing
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Psi.ExtensionsAPI
open JetBrains.ReSharper.Resources.Shell

type ReplaceUseWithLetFix(letNode: ILetBindings) =
    inherit FSharpQuickFixBase()

    new (warning: UseBindingsIllegalInModulesWarning) =
        ReplaceUseWithLetFix(warning.LetBindings)

    new (error: UseKeywordIllegalInPrimaryCtorError) =
        ReplaceUseWithLetFix(error.LetBindings)

    override x.Text = "Replace with 'let'"
    override x.IsAvailable _ = isValid letNode

    override x.ExecutePsiTransaction _ =
        let useKeyword = letNode.BindingKeyword
        Assertion.Assert(useKeyword.GetTokenType() == FSharpTokenType.USE,
                         sprintf "Expecting use, got: %O" (useKeyword.GetTokenType()))

        use writeLock = WriteLockCookie.Create(letNode.IsPhysical())
        replaceWithToken useKeyword FSharpTokenType.LET
