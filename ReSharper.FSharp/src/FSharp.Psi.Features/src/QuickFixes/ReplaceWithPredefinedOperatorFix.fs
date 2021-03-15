﻿namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes

open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.Highlightings
open JetBrains.ReSharper.Plugins.FSharp.Psi.Features.Daemon.QuickFixes
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.ReSharper.Psi.ExtensionsAPI
open JetBrains.ReSharper.Resources.Shell

type ReplaceWithPredefinedOperatorFix(error: AddingConstraintError) =
    inherit FSharpQuickFixBase()

    let ref = error.Expr.As<IReferenceExpr>()

    let fsharpOperator =
        match ref with
        | null -> SharedImplUtil.MISSING_DECLARATION_NAME
        | ref when ref.IsQualified -> SharedImplUtil.MISSING_DECLARATION_NAME
        | ref ->

        match ref.ShortName with
        | "==" -> "="
        | "!=" -> "<>"
        | _ -> SharedImplUtil.MISSING_DECLARATION_NAME

    override x.IsAvailable _ =
        isValid ref &&
        isNotNull (BinaryAppExprNavigator.GetByOperator(ref)) &&
        fsharpOperator <> SharedImplUtil.MISSING_DECLARATION_NAME

    override x.Text = $"Replace with '{fsharpOperator}'"

    override x.ExecutePsiTransaction _ =
        use writeCookie = WriteLockCookie.Create(ref.IsPhysical())
        use disableFormatter = new DisableCodeFormatter()

        ref.SetName(fsharpOperator) |> ignore
