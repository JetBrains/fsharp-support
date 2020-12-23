module Say

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of int
    | CaseWithMultipleFields of string * int

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch with
| TestUnion.CaseWithSingleField(1) -> failwith ""
| _ -> failwith ""

// A comment
let foo = 1