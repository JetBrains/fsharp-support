match [] with
| (_ & _) | _ -> ()
| (_ & _) & _ -> ()
| _ & (_ & _) -> ()

| (_ & _) as a -> ()
| (_ as a & _) -> ()
| ((_ as a) & _) -> ()
| _ & (_ as a) -> ()
