module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of int
    | CaseWithMultipleFields of string * int

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithSingleField(x) when x = 0 -> failwith ""
| TestUnion.CaseWithSingleField(_) -> failwith ""
| TestUnion.CaseWithMultipleFields(_) -> failwith ""

// A comment
let foo = 1