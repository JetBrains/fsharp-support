module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of bool

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithSingleField(true) -> failwith ""
| TestUnion.CaseWithSingleField(false) -> failwith ""

// A comment
let foo = 1