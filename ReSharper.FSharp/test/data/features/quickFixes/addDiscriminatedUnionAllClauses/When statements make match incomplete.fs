module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of int
    | CaseWithMultipleFields of string * int

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithSingleField(x) when x = 0 -> failwith ""
| TestUnion.CaseWithSingleField(x) when x = 100 -> failwith ""
| TestUnion.CaseWithMultipleFields(x, y) -> failwith ""
| TestUnion.FieldlessCase -> failwith ""

// A comment
let foo = 1