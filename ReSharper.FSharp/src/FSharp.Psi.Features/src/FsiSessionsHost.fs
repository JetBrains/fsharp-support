namespace JetBrains.ReSharper.Plugins.FSharp.Services.ContextActions

open System
open System.Diagnostics
open System.Collections.Generic
open System.Globalization
open System.IO
open System.Linq
open System.Text
open System.Threading
open JetBrains.Application.Settings
open JetBrains.DataFlow
open JetBrains.DocumentModel
open JetBrains.ProjectModel
open JetBrains.Platform.RdFramework.Base
open JetBrains.Platform.RdFramework.Util
open JetBrains.ReSharper.Feature.Services.Bulbs
open JetBrains.ReSharper.Feature.Services.ContextActions
open JetBrains.ReSharper.Feature.Services.Intentions
open JetBrains.ReSharper.Feature.Services.Resources
open JetBrains.ReSharper.Host.Features
open JetBrains.ReSharper.Host.Features.Toolset
open JetBrains.ReSharper.Plugins.FSharp.Services.Settings
open JetBrains.ReSharper.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi
open JetBrains.ReSharper.Plugins.FSharp.Psi.Tree
open JetBrains.Rider.Model
open JetBrains.TextControl
open JetBrains.UI.RichText
open JetBrains.Util
open JetBrains.Util.dataStructures.TypedIntrinsics

type SendToFsiActionType =
    | SendLine
    | SendSelection

[<SolutionComponent>]
type FsiSessionsHost(lifetime: Lifetime, solution: ISolution, solutionModel: SolutionModel, toolset: RiderSolutionToolset) as this =
    let rdFsiHost =
        match solutionModel.TryGetCurrentSolution() with
        | null -> failwith "Could not get fsi host"
        | solution -> solution.FSharpInteractiveHost

    let stringOption option arg = sprintf "--%s:%O" option arg
    let boolOption option arg = sprintf "--%s%s" option (if arg then "+" else "-")

    do
        rdFsiHost.RequestNewFsiSessionInfo.Set(this.GetNewFsiSessionInfo)
        let settings = solution.GetSettingsStore().SettingsStore.BindToContextLive(lifetime, ContextRange.ApplicationWide)

        let moveCaretOnSendLine = settings.GetValueProperty(lifetime, (fun (s: FsiOptions) -> s.MoveCaretOnSendLine))
        rdFsiHost.MoveCaretOnSendLine.Value <- moveCaretOnSendLine.Value
        moveCaretOnSendLine.FlowInto(lifetime, rdFsiHost.MoveCaretOnSendLine :> IRdProperty<_>)

        let copyRecentToEditor = settings.GetValueProperty(lifetime, (fun (s: FsiOptions) -> s.CopyRecentToEditor))
        rdFsiHost.MoveCaretOnSendLine.Value <- copyRecentToEditor.Value
        copyRecentToEditor.FlowInto(lifetime, rdFsiHost.CopyRecentToEditor :> IRdProperty<_>)

    member x.GetNewFsiSessionInfo(_) =
        let settings = solution.GetSettingsStore()
        let useFsiAnyCpu = settings.GetValue(fun (s: FsiOptions) -> s.UseAnyCpuVersion)

        // todo: move discover process to another place (and discover other F#-specific things like targets files)
        let fsiPath =
            if PlatformUtil.IsRunningUnderWindows then
                let fsiName = if useFsiAnyCpu then "fsiAnyCpu.exe" else "fsi.exe"
                let programFilesPath =
                    FileSystemPath.TryParse(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86))
                programFilesPath.Combine("Microsoft SDKs/F#").GetChildDirectories()
                |> Seq.choose (fun path ->
                    match Double.TryParse(path.Name, NumberStyles.Any, CultureInfo.InvariantCulture) with
                    | true, version ->
                        let fsiPath = path.Combine("Framework/v4.0").Combine(fsiName)
                        if not fsiPath.ExistsFile then None else
                        Some (version, fsiPath.FullPath)
                    | _ -> None)
                |> Seq.maxBy fst
                |> snd
            else
                let fsiName = if useFsiAnyCpu then "fsharpiAnyCpu" else "fsharpi"
                toolset.CurrentMonoRuntime.RootPath.Combine("bin").Combine(fsiName).FullPath

        let shadowCopyReferences = settings.GetValue(fun (s: FsiOptions) -> s.ShadowCopyReferences)
        let userArgs =
            settings.GetValue(fun (s: FsiOptions) -> s.FsiArgs).Split(' ')
            |> Seq.ofArray
            |> Seq.map (fun s -> s.Trim())
            |> Seq.filter (fun s -> not (s.IsEmpty()))
            
        let args =
            seq { 
                if PlatformUtil.IsRunningUnderWindows then
                    yield stringOption "fsi-server-output-codepage" Encoding.UTF8.CodePage
                    yield stringOption "fsi-server-input-codepage"  Encoding.UTF8.CodePage
                yield stringOption "fsi-server-lcid" Thread.CurrentThread.CurrentUICulture.LCID
                yield boolOption "shadowcopyreferences" shadowCopyReferences
                yield! userArgs
            }
        RdFsiSessionInfo(fsiPath, List(args))
