match (), () with
| a, _
| (_ as a), _ -> ()

match (), () with
| _, a
| _, (_ as a) -> ()

match (), (), () with
| _, a, _
| _, (_ as a), _ -> ()
