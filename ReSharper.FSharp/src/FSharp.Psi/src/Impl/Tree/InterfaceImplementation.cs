﻿using JetBrains.ReSharper.Plugins.FSharp.Psi.Tree;

namespace JetBrains.ReSharper.Plugins.FSharp.Psi.Impl.Tree
{
    internal partial class InterfaceImplementation
    {
        public INamedTypeExpression TypeExpression => InterfaceType;
    }
}