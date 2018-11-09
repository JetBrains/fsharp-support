package com.jetbrains.rider.ideaInterop.fileTypes.fsharp.psi.impl

import com.intellij.extapi.psi.PsiFileBase
import com.intellij.psi.FileViewProvider
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.FSharpFileType
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.FSharpLanguage
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.FSharpScriptFileType
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.FSharpScriptLanguage
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.psi.FSharpFile
import com.jetbrains.rider.ideaInterop.fileTypes.fsharp.psi.FSharpScript

class FSharpFileImpl(viewProvider: FileViewProvider) : FSharpFile, PsiFileBase(viewProvider, FSharpLanguage) {
    override fun getFileType() = FSharpFileType
}

class FSharpScriptImpl(viewProvider: FileViewProvider) : FSharpScript, PsiFileBase(viewProvider, FSharpScriptLanguage) {
    override fun getFileType() = FSharpScriptFileType
}