module JetBrains.ReSharper.Plugins.FSharp.Psi.Features.CodeCleanup.FSharpEditorConfig

open System
open JetBrains.Application
open JetBrains.Application.Settings
open JetBrains.Application.Settings.Calculated.Implementation
open JetBrains.DataFlow
open JetBrains.Diagnostics
open JetBrains.Lifetimes
open JetBrains.ReSharper.Plugins.FSharp.Services.Formatter
open JetBrains.ReSharper.Psi.EditorConfig

type FantomasSettingsConverter(fantomasSettingsEntry: SettingsIndexedEntry) =
  let fSharpPrefix = "fsharp_"

  let run (context: SettingsConvertContext<'a>) _ =
      let context = context.As<SettingsConvertContext<string>>()
      Assertion.AssertNotNull(context, "Context expected to be SettingsConvertContext<string>")

      for entry in context.SourceData.FindEntriesByPrefix(fSharpPrefix) do
        let setting = SettingIndex(fantomasSettingsEntry, entry.Key)
        context.Target.SetValue(setting, entry.Value)

      true

  interface IEditorConfigConverter with
    member val Category = "F#" with get, set
    member x.IsPropertySupported(property) = property.StartsWith(fSharpPrefix, StringComparison.Ordinal)
    member x.Convert(context) = run context null
    member x.ConvertAndCheck(context) = run context context
    member x.ReverseConvert _ = failwith "Not implemented"


[<ShellComponent>]
type FantomasSettingsConverterProvider(lifetime: Lifetime, schema: SettingsSchema) =
    let items = CollectionEvents<IEditorConfigConverter>(lifetime, $"{nameof FantomasSettingsConverterProvider}.Items")

    do
      let fantomasSettingsEntry = schema.GetIndexedEntry<_, _>(fun (key: FSharpFormatSettingsKey) -> key.FantomasSettings)
      items.Add(FantomasSettingsConverter(fantomasSettingsEntry))

    interface IProvider<IEditorConfigConverter> with
        member this.Items = items :> _
