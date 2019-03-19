namespace JetBrains.ReSharper.Plugins.FSharp.Common.Shim.FileSystem

open JetBrains.Application.Infra
open JetBrains.Lifetimes
open JetBrains.ProjectModel
open JetBrains.ReSharper.Plugins.FSharp
open JetBrains.Util

[<SolutionComponent>]
type AssemblyInfoShim
        (lifetime: Lifetime, fsSourceCache: FSharpSourceCache, assemblyExistsService: AssemblyExistsService,
         toolset: ISolutionToolset) =
    inherit DelegatingFileSystemShim(lifetime)

    let isSupported (path: FileSystemPath) =
        let extension = path.ExtensionNoDot
        extension = "dll" || extension = "exe"

    override x.GetLastWriteTime(path) =
        if isSupported path then assemblyExistsService.GetFileSystemData(path).LastWriteTimeUtc
        else base.GetLastWriteTime(path)

    override x.Exists(path) =
        if isSupported path then assemblyExistsService.GetFileSystemData(path).FileExists else
        base.Exists(path)

    override x.IsStableFile(path) =
        match toolset.CurrentBuildTool with
        | null -> base.IsStableFile(path)
        | buildTool -> buildTool.Directory.IsPrefixOf(path)
