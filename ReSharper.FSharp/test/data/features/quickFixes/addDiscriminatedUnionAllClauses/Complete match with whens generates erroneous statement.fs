module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of bool

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithSingleField(x) when x = true -> failwith ""
| TestUnion.CaseWithSingleField(x) when x = false -> failwith ""

// A comment
let foo = 1