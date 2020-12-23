module Say

type InnerTestUnion =
    | InnerField of string list

type TestUnion =
    | FieldlessCase
    | CaseWithSingleField of InnerTestUnion

let unionCaseMatch = TestUnion.FieldlessCase
match unionCaseMatch{caret} with
| TestUnion.CaseWithSingleField(InnerTestUnion.InnerField([])) -> failwith ""
| TestUnion.CaseWithSingleField(InnerTestUnion.InnerField(["string"])) -> failwith ""
| TestUnion.FieldlessCase -> failwith ""

// A comment
let foo = 1