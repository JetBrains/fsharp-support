namespace JetBrains.ReSharper.Plugins.FSharp.Common.Util

open JetBrains.ReSharper.Psi.ExtensionsAPI

[<RequireQualifiedAccess>]
type PartKind =
    | Class = 0
    | Struct = 1
    | Interface = 2
    | Enum = 3


[<Struct>]
type NameAndParametersCound =
    { Name: string
      ParametersCount: int }


[<RequireQualifiedAccess>]
type TypeAugmentation =
    | TypePart of compiledName: string * parametersCount: int * kind: PartKind
    | Extension

    member x.CompiledName =
        match x with
        | TypePart(name, _, _) -> name
        | _ -> SharedImplUtil.MISSING_DECLARATION_NAME

    member x.PartKind =
        match x with
        | TypePart(_, _, kind) -> kind
        | _ -> failwith "Not a type part."
