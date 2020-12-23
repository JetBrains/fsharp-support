module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of int
    | CaseWithMultipleFields of string * int

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with

// A comment
let foo = 1