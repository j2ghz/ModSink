module Utilities

[<Struct>]
type OptionalBuilder =
  member __.Bind(opt, binder) =
    match opt with
    | Some value -> binder value
    | None -> None
  member __.Return(value) =
    Some value

let optional = OptionalBuilder()

module Async =
    let rec sequenceList items =
        async {
            let! i = items |> List.head
            let! is = items |> List.tail |> sequenceList
            return i :: is
        }