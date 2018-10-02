module HashingTests

open Xunit
open FsCheck
open System.IO

[<Fact>]
let ``Returns a valid hash``() =
    let hash arr =
        use stream = new MemoryStream(arr,false)
        let result =
            Hashing.getHash stream
            |> Async.RunSynchronously
        result.Length = (Array.length arr |> int64)

    Check.QuickThrowOnFailure hash