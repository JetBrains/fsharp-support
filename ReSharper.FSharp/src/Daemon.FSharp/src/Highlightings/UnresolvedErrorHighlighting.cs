﻿using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace JetBrains.ReSharper.Plugins.FSharp.Daemon.Cs.Highlightings
{
  [StaticSeverityHighlighting(Severity.ERROR, HighlightingGroupIds.IdentifierHighlightingsGroup,
    AttributeId = HighlightingAttributeIds.UNRESOLVED_ERROR_ATTRIBUTE, OverlapResolve = OverlapResolveKind.UNRESOLVED_ERROR)]
  public class UnresolvedHighlighting : ErrorOrWarningHighlightingBase
  {
    public UnresolvedHighlighting([NotNull] string message, DocumentRange range) : base(message, range)
    {
    }
  }
}