module Module

type TT1 = (int * int)
type TT2 = (int * int) * int
type TT3 = int * (int * int)
type TT4 = (int -> int) * (int -> int)

type TT1 = (struct (int * int))
type TT2 = ((struct (int * int)))
