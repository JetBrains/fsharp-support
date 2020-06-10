package com.jetbrains.rider.plugins.fsharp.services.completion

import com.intellij.codeInsight.completion.CompletionType
import com.intellij.openapi.editor.Editor
import com.intellij.psi.PsiFile
import com.jetbrains.rdclient.completion.CompletionSessionHeuristics
import com.jetbrains.rdclient.completion.ICompletionSessionStrategy

class FSharpCompletionStrategy : ICompletionSessionStrategy {
    override fun shouldForbidCompletion(editor: Editor, type: CompletionType) = editor.selectionModel.hasSelection()

    override fun shouldRescheduleCompletion(prefix: String, psiFile: PsiFile, char: Char?, offset: Int) =
            prefix.isEmpty() ||
                    CompletionSessionHeuristics.getInstance(psiFile.project).shouldRescheduleDefaultStrategy(char)
}
