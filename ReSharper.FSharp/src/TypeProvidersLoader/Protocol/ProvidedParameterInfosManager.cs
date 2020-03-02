﻿using System;
using System.Collections.Generic;
using JetBrains.Rider.FSharp.TypeProvidersProtocol.Client;
using Microsoft.FSharp.Core.CompilerServices;
using static FSharp.Compiler.ExtensionTyping;

namespace JetBrains.ReSharper.Plugins.FSharp.TypeProvidersLoader.Protocol
{
  public class
    ProvidedParametersManager : OutOfProcessProtocolManagerBase<ProvidedParameterInfo, RdProvidedParameterInfo>
  {
    private readonly IOutOfProcessProtocolManager<ProvidedType, RdProvidedType> myProvidedTypesManager;

    public ProvidedParametersManager(IOutOfProcessProtocolManager<ProvidedType, RdProvidedType> providedTypesManager) :
      base(new ProvidedParameterInfoEqualityComparer())
    {
      myProvidedTypesManager = providedTypesManager;
    }

    protected override RdProvidedParameterInfo CreateProcessModel(
      ProvidedParameterInfo providedNativeModel,
      ITypeProvider providedModelOwner)
    {
      var parameterModel = new RdProvidedParameterInfo(providedNativeModel.Name,
        myProvidedTypesManager.Register(providedNativeModel.ParameterType, providedModelOwner),
        providedNativeModel.IsIn,
        providedNativeModel.IsOut,
        providedNativeModel.IsOptional,
        providedNativeModel.HasDefaultValue);

      return parameterModel;
    }
  }

  internal class ProvidedParameterInfoEqualityComparer : IEqualityComparer<ProvidedParameterInfo>
  {
    public bool Equals(ProvidedParameterInfo x, ProvidedParameterInfo y)
    {
      return ReferenceEquals(x, y);
    }

    public int GetHashCode(ProvidedParameterInfo obj)
    {
      return obj.Name.GetHashCode();
    }
  }
}
