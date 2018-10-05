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

module List =
    let rec asyncSequence items =
        async {
            let! i = items |> List.head
            let! is = items |> List.tail |> asyncSequence
            return i :: is
        }
module Seq =
    let rec asyncSequence items =
        async {
            let! i = items |> Seq.head
            let! is = items |> Seq.tail |> asyncSequence
            return Seq.append (seq { yield i }) is
        }