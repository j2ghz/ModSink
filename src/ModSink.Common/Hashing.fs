module Hashing

open System.Data.HashFunction.xxHash

let xxhash =
    let config = new xxHashConfig(HashSizeInBits = 64)
    xxHashFactory.Instance.Create(config)

let getHash stream =
    async {
        let! hash = xxhash.ComputeHashAsync stream
                    |> Async.AwaitTask
        return {HashValue=hash.Hash;Length=stream.Length}
    }