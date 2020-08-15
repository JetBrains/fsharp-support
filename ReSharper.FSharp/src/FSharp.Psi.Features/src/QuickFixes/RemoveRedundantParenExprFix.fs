﻿namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes

open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
open JetBrains.ReSharper.Psi.ExtensionsAPI
open JetBrains.ReSharper.Psi.ExtensionsAPI.Tree
open JetBrains.ReSharper.Psi.Tree
open JetBrains.ReSharper.Resources.Shell

type RemoveRedundantParenExprFix(warning: RedundantParenExprWarning) =
    inherit FSharpQuickFixBase()

    let parenExpr = warning.ParenExpr
    let innerExpr = parenExpr.InnerExpression

    override x.Text = "Remove redundant parens"

    override x.IsAvailable _ =
        isValid parenExpr && isValid innerExpr

    override x.ExecutePsiTransaction _ =
        use writeCookie = WriteLockCookie.Create(parenExpr.IsPhysical())
        use disableFormatter = new DisableCodeFormatter()

        let parenExprIndent = parenExpr.Indent
        let innerExprIndent = innerExpr.Indent
        let indentDiff = parenExprIndent - innerExprIndent

        let prevSibling = parenExpr.PrevSibling
        if isNotNull prevSibling && not (prevSibling.IsWhitespaceToken()) then 
            ModificationUtil.AddChildBefore(parenExpr, Whitespace()) |> ignore

        let parent = parenExpr.Parent
        if isNotNull parent && isNotNull parent.NextSibling && not (parent.NextSibling.IsWhitespaceToken()) then 
            ModificationUtil.AddChildAfter(parent, Whitespace()) |> ignore

        let expr = ModificationUtil.ReplaceChild(parenExpr, innerExpr.Copy())
        shiftExpr indentDiff expr
