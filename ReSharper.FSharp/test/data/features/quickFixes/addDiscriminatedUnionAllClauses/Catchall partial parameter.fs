module Say

type TestUnion =
    | CaseWithMultipleFields of string * int * int

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithMultipleFields("string", _) -> failwith ""

// A comment
let foo = 1