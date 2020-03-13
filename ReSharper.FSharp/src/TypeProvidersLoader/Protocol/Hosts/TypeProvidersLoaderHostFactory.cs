﻿using System.Linq;
using JetBrains.Lifetimes;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Plugins.FSharp.TypeProvidersLoader.Protocol.ModelCreators;
using JetBrains.Rider.FSharp.TypeProvidersProtocol.Client;
using Microsoft.FSharp.Core.CompilerServices;

namespace JetBrains.ReSharper.Plugins.FSharp.TypeProvidersLoader.Protocol.Hosts
{
  public class TypeProvidersLoaderHostFactory : IOutOfProcessHostFactory<RdFSharpTypeProvidersLoaderModel>
  {
    private readonly ITypeProvidersLoader myTypeProvidersLoader;
    private readonly IProvidedRdModelsCreator<ITypeProvider, RdTypeProvider> myTypeProvidersCreator;

    public TypeProvidersLoaderHostFactory(ITypeProvidersLoader typeProvidersLoader,
      IProvidedRdModelsCreator<ITypeProvider, RdTypeProvider> typeProvidersCreator)
    {
      myTypeProvidersLoader = typeProvidersLoader;
      myTypeProvidersCreator = typeProvidersCreator;
    }

    public void Initialize(RdFSharpTypeProvidersLoaderModel model)
    {
      model.InstantiateTypeProvidersOfAssembly.Set(InstantiateTypeProvidersOfAssembly);
    }

    private RdTask<RdTypeProvider[]> InstantiateTypeProvidersOfAssembly(Lifetime lifetime,
      InstantiateTypeProvidersOfAssemblyParameters @params)
    {
      var instantiateResults = myTypeProvidersLoader.InstantiateTypeProvidersOfAssembly(@params)
        .Select(t => myTypeProvidersCreator.CreateRdModel(t, -1))
        .ToArray();
      return RdTask<RdTypeProvider[]>.Successful(instantiateResults);
    }
  }
}
