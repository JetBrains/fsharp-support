module Module

let (    |  B  |    _ | ) x =
    if x then Some () else None

match true with
| B{caret}
| _ -> ()
