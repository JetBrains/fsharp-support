package com.jetbrains.rider.plugins.fsharp.services.fsi

import com.intellij.openapi.project.Project
import com.jetbrains.rd.platform.util.getComponent
import com.jetbrains.rd.platform.util.lifetime
import com.jetbrains.rd.util.lifetime.Lifetime
import com.jetbrains.rider.model.rdFSharpModel
import com.jetbrains.rider.projectView.actions.ProjectViewActionBase
import com.jetbrains.rider.projectView.nodes.ProjectModelNode
import com.jetbrains.rider.projectView.nodes.isProject
import com.jetbrains.rider.projectView.solution

class SendProjectReferencesToFsiAction : ProjectViewActionBase("Send project references", "Send project references") {
    override fun actionPerformedInternal(item: ProjectModelNode, project: Project) {
        val fSharpInteractiveHost = project.solution.rdFSharpModel.fSharpInteractiveHost
        fSharpInteractiveHost.getProjectReferences.start(project.lifetime, item.id).result.advise(Lifetime.Eternal) { result ->
            val text =
                    result.unwrap().joinToString("\n") { "#r @\"$it\"" } +
                            "\n" +
                            "# 1 \"stdin\"\n;;\n"
            project.getComponent<FsiHost>().sendToFsi(text, text, false)
        }
    }

    override fun getItemInternal(item: ProjectModelNode) =
            if (item.isProject()) item else null
}
