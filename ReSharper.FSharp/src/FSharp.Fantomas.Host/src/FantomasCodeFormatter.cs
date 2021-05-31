using System;
using System.Linq;
using Fantomas;
using FSharp.Compiler;
using FSharp.Compiler.SourceCodeServices;
using JetBrains.ReSharper.Plugins.FSharp.Fantomas.Server;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Reflection;
using FormatConfig = Fantomas.FormatConfig.FormatConfig;
using FSharpType = Microsoft.FSharp.Reflection.FSharpType;
using Range = FSharp.Compiler.Range.range;

namespace JetBrains.ReSharper.Plugins.FSharp.Fantomas.Host
{
  // TODO: collect used Fantomas versions
  internal class FantomasCodeFormatter
  {
    private readonly FSharpChecker myChecker =
      FSharpChecker.Create(null, null, null, null, null, null, null, null);

    private static readonly FormatConfig DefaultFormatConfig = FormatConfig.Default;
    private static readonly Type FormatConfigType = typeof(FormatConfig);

    private static readonly (string Name, object Value)[] FormatConfigFields =
      FSharpType.GetRecordFields(FormatConfigType, null)
        .Select(x => x.Name)
        .Zip(FSharpValue.GetRecordFields(DefaultFormatConfig, null), (name, value) => (name, value))
        .ToArray();

    public static readonly (string Name, object Value)[] EditorConfigFields =
      FormatConfigFields.Where(t => t.Value is int || t.Value is bool).ToArray();

    public string FormatSelection(RdFormatSelectionArgs args) =>
      FSharpAsync.StartAsTask(
          CodeFormatter.FormatSelectionAsync(args.FileName, Convert(args.Range),
            SourceOrigin.SourceOrigin.NewSourceString(args.Source), Convert(args.FormatConfig),
            Convert(args.ParsingOptions), myChecker), null, null)
        .Result.Replace("\r\n", args.NewLineText);

    public string FormatDocument(RdFormatDocumentArgs args) =>
      FSharpAsync.StartAsTask(
          CodeFormatter.FormatDocumentAsync(args.FileName, SourceOrigin.SourceOrigin.NewSourceString(args.Source),
            Convert(args.FormatConfig), Convert(args.ParsingOptions), myChecker), null, null)
        .Result.Replace("\r\n", args.NewLineText);

    private static Range Convert(RdFcsRange range) =>
      CodeFormatter.MakeRange(range.FileName, range.StartLine, range.StartCol, range.EndLine, range.EndCol);

    private static FSharpParsingOptions Convert(RdFcsParsingOptions options) =>
      new FSharpParsingOptions(new[] {options.LastSourceFile},
        ListModule.OfArray(options.ConditionalCompilationDefines), ErrorLogger.FSharpErrorSeverityOptions.Default,
        false, options.LightSyntax, false, options.IsExe);

    private static FormatConfig Convert(string[] riderFormatConfigValues)
    {
      var riderFormatConfigDict =
        EditorConfigFields
          .Zip(riderFormatConfigValues,
            (field, valueData) =>
              (field.Name, Value: valueData == ""
                ? field.Value
                : field.Value switch
                {
                  int _ => int.Parse(valueData),
                  bool _ => bool.Parse(valueData),
                  { } x => throw new InvalidOperationException($"Unexpected FormatConfig field {field.Name} = '{x}'")
                }))
          .ToDictionary(x => x.Name, x => x.Value);

      var formatConfigValues =
        FormatConfigFields
          .Select(field => riderFormatConfigDict.TryGetValue(field.Name, out var value) ? value : field.Value)
          .ToArray();

      return FSharpValue.MakeRecord(FormatConfigType, formatConfigValues, null) as FormatConfig;
    }
  }
}
