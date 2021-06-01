import org.jetbrains.intellij.tasks.BuildSearchableOptionsTask
import org.jetbrains.intellij.tasks.RunIdeTask
import org.jetbrains.intellij.tasks.PrepareSandboxTask

plugins {
    id("org.jetbrains.intellij")
}

var baseVersion = ext.properties["build.baseVersion"]

intellij {
    type = "RD"
    localPath = file("../build/riderRD-$baseVersion-SNAPSHOT").absolutePath
    instrumentCode = false
    downloadSources = false
}

java {
    sourceCompatibility = JavaVersion.VERSION_1_8
    targetCompatibility = JavaVersion.VERSION_1_8
}

val buildCounter = ext.properties["build.number"] ?: "9999"
version = "$baseVersion.$buildCounter"

val repoRoot = projectDir.parentFile.parentFile!!
val resharperPluginPath = File(repoRoot, "ReSharper.FSharp")
val buildConfiguration = ext.properties["BuildConfiguration"] ?: "Debug"

val pluginFiles = listOf(
        "FSharp.Fantomas.Protocol/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Fantomas.Protocol.dll",
        "FSharp.Fantomas.Protocol/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Fantomas.Protocol.pdb",
        "FSharp.Fantomas.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Fantomas.Host.exe",
        "FSharp.Fantomas.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Fantomas.Host.runtimeconfig.json",
        "FSharp.Fantomas.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Fantomas.Host.pdb",
        "FSharp.Fantomas.Host/bin/$buildConfiguration/net461/Fantomas.dll")

tasks {
    withType<BuildSearchableOptionsTask> {
        enabled = false
    }

    withType<RunIdeTask> {
        enabled = false
    }

    withType<PrepareSandboxTask> {
        val files = pluginFiles.map { "$resharperPluginPath/src/$it" }

        files.forEach {
            from(it) { into("${intellij.pluginName}/dotnet") }
        }

        doLast {
            files.forEach {
                val file = file(it)
                if (!file.exists()) throw RuntimeException("File $file does not exist")
                logger.warn("$name: ${file.name} -> $destinationDir/${intellij.pluginName}/dotnet")
            }
        }
    }
}
