﻿using FSharp.Compiler.EditorServices;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.Feature.Services.ParameterInfo;
using JetBrains.ReSharper.Plugins.FSharp.Util;
using JetBrains.UI.RichText;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Features.ParameterInfo
{
  public class FSharpParameterInfoCandidate : FSharpParameterInfoCandidateBase
  {
    private readonly MethodGroupItem myCandidate;

    public FSharpParameterInfoCandidate(MethodGroupItem candidate, bool isFiltered) : base(isFiltered)
    {
      myCandidate = candidate;
    }

    public override RichText GetSignature(string[] namedArguments, AnnotationsDisplayKind showAnnotations,
      out TextRange[] parameterRanges, out int[] mapToOriginalOrder, out ExtensionMethodInfo extensionMethodInfo)
    {
      var parameters = myCandidate.Parameters;
      var text = new RichText("(");
      var newParameterRanges = new TextRange[parameters.Length];
      var parametersOrder = SortParameters(parameters, namedArguments, out var orderChanged);

      if (parameters.IsEmpty())
        text.Append("<no parameters>");
      else
        for (var i = 0; i < parameters.Length; i++)
        {
          var paramRangeStart = text.Length;
          if (orderChanged)
            text.Append("[");
          text.Append(parameters[parametersOrder[i]].Display.ToRichText());
          if (orderChanged)
            text.Append("]");
          var paramRangeEnd = text.Length;

          newParameterRanges[i] = new TextRange(paramRangeStart, paramRangeEnd);
          if (i < parameters.Length - 1)
            text.Append(", ");
        }

      text.Append(")" + myCandidate.ReturnTypeText.ToRichText());

      extensionMethodInfo = ExtensionMethodInfo.NoExtension;
      parameterRanges = newParameterRanges;
      mapToOriginalOrder = parametersOrder;
      return text;
    }

    private static int[] SortParameters(MethodGroupItemParameter[] parameters, string[] namedArguments,
      out bool orderChanged)
    {
      var usedParams = new bool[parameters.Length];
      var originalOrder = new int[parameters.Length];
      var findNames = true;
      orderChanged = false;
      for (var i = 0; i < parameters.Length; i++)
        if (findNames && namedArguments.Length > i)
        {
          // try to find suitable place
          var name = namedArguments[i];
          for (var j = 0; j < parameters.Length; j++)
          {
            if (!usedParams[j] && parameters[j].ParameterName == name)
            {
              originalOrder[i] = j;
              usedParams[j] = true;
              if (i != j) orderChanged = true;
              break;
            }
            if (j == parameters.Length - 1)
            {
              // name wasn't found, place rest unused params
              findNames = false;
              for (var k = 0; k < parameters.Length; k++)
              {
                if (usedParams[j]) continue;
                originalOrder[i] = k;
                usedParams[k] = true;
                break;
              }
            }
          }
        }
        else
          for (var j = 0; j < parameters.Length; j++)
          {
            if (usedParams[j]) continue;
            originalOrder[i] = j;
            usedParams[j] = true;
            break;
          }
      return originalOrder;
    }

    public override int PositionalParameterCount => myCandidate.Parameters.Length;
  }
}