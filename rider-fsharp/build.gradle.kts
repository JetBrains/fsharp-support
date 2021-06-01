import com.jetbrains.rd.generator.gradle.RdGenExtension
import org.gradle.api.tasks.testing.logging.TestExceptionFormat
import org.jetbrains.grammarkit.tasks.GenerateLexer
import org.jetbrains.intellij.IntelliJPlugin
import org.jetbrains.intellij.tasks.PrepareSandboxTask
import org.jetbrains.intellij.tasks.RunIdeTask
import org.jetbrains.kotlin.daemon.common.toHexString
import org.jetbrains.kotlin.gradle.tasks.KotlinCompile
import java.net.URI

buildscript {
    repositories {
        maven { setUrl("https://cache-redirector.jetbrains.com/www.myget.org/F/rd-snapshots/maven") }
        maven { setUrl("https://cache-redirector.jetbrains.com/dl.bintray.com/kotlin/kotlin-eap") }
        maven { setUrl("https://cache-redirector.jetbrains.com/repo.maven.apache.org/maven2")}
        maven { setUrl("https://jetbrains.bintray.com/intellij-plugin-service") }
    }
    dependencies {
        // https://www.myget.org/feed/rd-snapshots/package/maven/com.jetbrains.rd/rd-gen
        classpath("com.jetbrains.rd:rd-gen:0.211.238")
    }
}

repositories {
    maven { setUrl("https://cache-redirector.jetbrains.com/repo.maven.org/maven2")}
    maven { setUrl("https://cache-redirector.jetbrains.com/repo.maven.apache.org/maven2")}
}

plugins {
    id("org.jetbrains.intellij") version "0.7.2" // https://github.com/JetBrains/gradle-intellij-plugin/releases
    id("org.jetbrains.grammarkit") version "2018.1.7"
    id("me.filippov.gradle.jvm.wrapper") version "0.9.3"
    kotlin("jvm") version "1.4.10"
}

apply {
    plugin("kotlin")
    plugin("com.jetbrains.rdgen")
    plugin("org.jetbrains.grammarkit")
}

java {
    sourceCompatibility = JavaVersion.VERSION_1_8
    targetCompatibility = JavaVersion.VERSION_1_8
}


val baseVersion = ext.properties["build.baseVersion"]
val buildCounter = ext.properties["build.number"] ?: "9999"
version = "$baseVersion.$buildCounter"

intellij {
    type = "RD"

    // Download a version of Rider to compile and run with. Either set `version` to
    // 'LATEST-TRUNK-SNAPSHOT' or 'LATEST-EAP-SNAPSHOT' or a known version.
    // This will download from www.jetbrains.com/intellij-repository/snapshots or
    // www.jetbrains.com/intellij-repository/releases, respectively.
    // Note that there's no guarantee that these are kept up to date
    // version = 'LATEST-TRUNK-SNAPSHOT'
    // If the build isn't available in intellij-repository, use an installed version via `localPath`
    // localPath = '/Users/matt/Library/Application Support/JetBrains/Toolbox/apps/Rider/ch-1/171.4089.265/Rider EAP.app/Contents'
    // localPath = "C:\\Users\\Ivan.Shakhov\\AppData\\Local\\JetBrains\\Toolbox\\apps\\Rider\\ch-0\\171.4456.459"
    // localPath = "C:\\Users\\ivan.pashchenko\\AppData\\Local\\JetBrains\\Toolbox\\apps\\Rider\\ch-0\\dev"
    // localPath 'build/riderRD-173-SNAPSHOT'

    val dir = file("build/rider")
    if (dir.exists()) {
        logger.lifecycle("*** Using Rider SDK from local path " + dir.absolutePath)
        localPath = dir.absolutePath
    } else {
        logger.lifecycle("*** Using Rider SDK from intellij-snapshots repository")
        version = "$baseVersion-SNAPSHOT"
    }

    instrumentCode = false
    downloadSources = false

    // rider-plugins-appender - workaround for https://youtrack.jetbrains.com/issue/IDEA-179607
    setPlugins(project(":rider-fantomas"), "rider-plugins-appender")
}

repositories.forEach {
    fun replaceWithCacheRedirector(u: URI): URI {
        val cacheHost = "cache-redirector.jetbrains.com"
        return if (u.scheme.startsWith("http") && u.host != cacheHost)
            URI("https", cacheHost, "/${u.host}/${u.path}", u.query, u.fragment)
        else u
    }

    when (it) {
        is MavenArtifactRepository -> {
            it.url = replaceWithCacheRedirector(it.url)
        }
        is IvyArtifactRepository -> {
            it.url = replaceWithCacheRedirector(it.url)
        }
    }
}

val repoRoot = projectDir.parentFile!!
val resharperPluginPath = File(repoRoot, "ReSharper.FSharp")
val buildConfiguration = ext.properties["BuildConfiguration"] ?: "Debug"

val libFiles = listOf(
        "FSharp.Common/bin/$buildConfiguration/net461/FSharp.Core.dll",
        "FSharp.Common/bin/$buildConfiguration/net461/FSharp.Core.xml",
        "FSharp.Common/bin/$buildConfiguration/net461/FSharp.Compiler.Service.dll", // todo: add pdb after next repack
        "FSharp.Common/bin/$buildConfiguration/net461/FSharp.DependencyManager.Nuget.dll",
        "FSharp.Common/bin/$buildConfiguration/net461/FSharp.Compiler.Interactive.Settings.dll")

val pluginFiles = listOf(
        "FSharp.ProjectModelBase/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.ProjectModelBase",
        "FSharp.Common/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Common",
        "FSharp.Psi/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Psi",
        "FSharp.Psi.Features/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.Psi.Features")

val typeProvidersFiles = listOf(
        "FSharp.TypeProviders.Protocol/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Protocol.dll",
        "FSharp.TypeProviders.Protocol/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Protocol.pdb",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Host.exe",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Host.pdb",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/net461/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Host.exe.config",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Host.Core.dll",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/JetBrains.ReSharper.Plugins.FSharp.TypeProviders.Host.Core.pdb",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/tploader3.win.runtimeconfig.json",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/tploader3.unix.runtimeconfig.json",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/tploader5.win.runtimeconfig.json",
        "FSharp.TypeProviders.Host/bin/$buildConfiguration/netcoreapp3.1/tploader5.unix.runtimeconfig.json")

val dotNetSdkPath by lazy {
    val sdkPath = intellij.ideaDependency.classes.resolve("lib").resolve("DotNetSdkForRdPlugins")
    if (sdkPath.isDirectory.not()) error("$sdkPath does not exist or not a directory")

    println("SDK path: $sdkPath")
    return@lazy sdkPath
}

val nugetConfigPath = File(repoRoot, "NuGet.Config")
val dotNetSdkPathPropsPath = File("build", "DotNetSdkPath.generated.props")
val backendLexerSources = "$repoRoot/rider-fsharp/build/backend-lexer-sources/"

val riderFSharpTargetsGroup = "rider-fsharp"

fun File.writeTextIfChanged(content: String) {
    val bytes = content.toByteArray()

    if (!exists() || readBytes().toHexString() != bytes.toHexString()) {
        println("Writing $path")
        writeBytes(bytes)
    }
}

configure<RdGenExtension> {
    val csOutput = File(repoRoot, "ReSharper.FSharp/src/FSharp.ProjectModelBase/src/Protocol")
    val ktOutput = File(repoRoot, "rider-fsharp/src/main/java/com/jetbrains/rider/plugins/fsharp/protocol")

    val typeProviderClientOutput = File(repoRoot, "ReSharper.FSharp/src/FSharp.TypeProviders.Protocol/src/Client")
    val typeProviderServerOutput = File(repoRoot, "ReSharper.FSharp/src/FSharp.TypeProviders.Protocol/src/Server")

    val fantomasServerOutput = File(repoRoot, "ReSharper.FSharp/src/FSharp.Fantomas.Protocol/src/Server")
    val fantomasClientOutput = File(repoRoot, "ReSharper.FSharp/src/FSharp.Fantomas.Protocol/src/Client")

    verbose = true
    hashFolder = "build/rdgen"
    logger.info("Configuring rdgen params")
    classpath({
        logger.info("Calculating classpath for rdgen, intellij.ideaDependency is ${intellij.ideaDependency}")
        val sdkPath = intellij.ideaDependency.classes
        val rdLibDirectory = File(sdkPath, "lib/rd").canonicalFile

        "$rdLibDirectory/rider-model.jar"
    })
    sources(File(repoRoot, "rider-fsharp/protocol/src/kotlin/model"))
    packages = "model"

    generator {
        language = "kotlin"
        transform = "asis"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "com.jetbrains.rider.model"
        directory = "$ktOutput"
    }

    generator {
        language = "csharp"
        transform = "reversed"
        root = "com.jetbrains.rider.model.nova.ide.IdeRoot"
        namespace = "JetBrains.Rider.Model"
        directory = "$csOutput"
    }

    generator {
        language = "csharp"
        transform = "asis"
        root = "model.RdFSharpTypeProvidersModel"
        namespace = "JetBrains.Rider.FSharp.TypeProviders.Protocol.Client"
        directory = "$typeProviderClientOutput"
    }
    generator {
        language = "csharp"
        transform = "reversed"
        root = "model.RdFSharpTypeProvidersModel"
        namespace = "JetBrains.Rider.FSharp.TypeProviders.Protocol.Server"
        directory = "$typeProviderServerOutput"
    }

    generator {
        language = "csharp"
        transform = "asis"
        root = "model.RdFantomasModel"
        namespace = "JetBrains.ReSharper.Plugins.FSharp.Fantomas.Client"
        directory = "$fantomasClientOutput"
    }
    generator {
        language = "csharp"
        transform = "reversed"
        root = "model.RdFantomasModel"
        namespace = "JetBrains.ReSharper.Plugins.FSharp.Fantomas.Server"
        directory = "$fantomasServerOutput"
    }
}

tasks {
    withType<PrepareSandboxTask> {
        var files = libFiles + pluginFiles.map { "$it.dll" } + pluginFiles.map { "$it.pdb" } + typeProvidersFiles
        files = files.map { "$resharperPluginPath/src/$it" }

        if (name == IntelliJPlugin.PREPARE_TESTING_SANDBOX_TASK_NAME) {
            val testHostPath = "$resharperPluginPath/test/src/FSharp.Tests.Host/bin/$buildConfiguration/net461"
            val testHostName = "$testHostPath/JetBrains.ReSharper.Plugins.FSharp.Tests.Host"
            files = files + listOf("$testHostName.dll", "$testHostName.pdb")
        }

        files.forEach {
            from(it) { into("${intellij.pluginName}/dotnet") }
        }

        into("${intellij.pluginName}/projectTemplates") {
            from("projectTemplates")
        }

        doLast {
            files.forEach {
                val file = file(it)
                if (!file.exists()) throw RuntimeException("File $file does not exist")
                logger.warn("$name: ${file.name} -> $destinationDir/${intellij.pluginName}/dotnet")
            }
        }
    }

    // Initially introduced in:
    // https://github.com/JetBrains/ForTea/blob/master/Frontend/build.gradle.kts
    withType<RunIdeTask> {
        // Match Rider's default heap size of 1.5Gb (default for runIde is 512Mb)
        maxHeapSize = "1500m"
    }

    val resetLexerDirectory = create("resetLexerDirectory") {
        doFirst {
            File(backendLexerSources).deleteRecursively()
            File(backendLexerSources).mkdirs()
        }
    }

    // Cannot use ordinary copy here, because it requires eager evaluation of locations
    val copyUnicodeLex = create("copyUnicodeLex") {
        dependsOn(resetLexerDirectory)
        doFirst {
            val libPath = File("$dotNetSdkPath").parent
            File(libPath, "ReSharperHost/PsiTasks").listFiles { it -> it.extension == "lex" }!!.forEach {
                println(it)
                it.copyTo(File(backendLexerSources, it.name))
            }
        }
    }

    val copyBackendLexerSources = create<Copy>("copyBackendLexerSources") {
        dependsOn(resetLexerDirectory)
        from("$resharperPluginPath/src/FSharp.Psi/src/Parsing/Lexing") {
            include("*.lex")
        }
        into(backendLexerSources)
    }

    val generateFSharpLexer = task<GenerateLexer>("generateFSharpLexer") {
        dependsOn(copyBackendLexerSources, copyUnicodeLex)
        source = "src/main/java/com/jetbrains/rider/ideaInterop/fileTypes/fsharp/lexer/_FSharpLexer.flex"
        targetDir = "src/main/java/com/jetbrains/rider/ideaInterop/fileTypes/fsharp/lexer"
        targetClass = "_FSharpLexer"
        purgeOldFiles = true
    }

    withType<KotlinCompile> {
        kotlinOptions.jvmTarget = "1.8"
        dependsOn(generateFSharpLexer, "rdgen")
    }

    withType<Test> {
        useTestNG()
        testLogging {
            showStandardStreams = true
            exceptionFormat = TestExceptionFormat.FULL
        }
        val rerunSuccessfulTests = false
        outputs.upToDateWhen { !rerunSuccessfulTests }
        ignoreFailures = true
    }

    create("writeDotNetSdkPathProps") {
        group = riderFSharpTargetsGroup
        doLast {
            dotNetSdkPathPropsPath.writeTextIfChanged("""<Project>
  <PropertyGroup>
    <DotNetSdkPath>$dotNetSdkPath</DotNetSdkPath>
  </PropertyGroup>
</Project>
""")
        }

        getByName("buildSearchableOptions") {
            enabled = buildConfiguration == "Release"
        }
    }

    create("writeNuGetConfig") {
        group = riderFSharpTargetsGroup
        doLast {
            nugetConfigPath.writeTextIfChanged("""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="resharper-sdk" value="$dotNetSdkPath" />
  </packageSources>
</configuration>
""")
        }
    }

    getByName("assemble") {
        doLast {
            logger.lifecycle("Plugin version: $version")
            logger.lifecycle("##teamcity[buildNumber '$version']")
        }
    }

    create("prepare") {
        group = riderFSharpTargetsGroup
        dependsOn("rdgen", "writeNuGetConfig", "writeDotNetSdkPathProps")
    }

    create("buildReSharperPlugin") {
        group = riderFSharpTargetsGroup
        dependsOn("prepare")
        doLast {
            exec {
                executable = "msbuild"
                args = listOf("$resharperPluginPath/ReSharper.FSharp.sln")
            }
        }
    }

    task("listrepos"){
        doLast {
            logger.lifecycle("Repositories:")
            project.repositories.forEach {
                when (it) {
                    is MavenArtifactRepository -> logger.lifecycle("Name: ${it.name}, url: ${it.url}")
                    is IvyArtifactRepository -> logger.lifecycle("Name: ${it.name}, url: ${it.url}")
                    else -> logger.lifecycle("Name: ${it.name}, $it")
                }
            }
        }
    }
}

defaultTasks("prepare")
