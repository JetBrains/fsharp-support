module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of int
    | CaseWithMultipleFields of string * int

// Not available on Union literal
match TestUnion.FieldlessCase with