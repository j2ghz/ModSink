module RepoUtilities
open Microsoft.FSharpLu.File
open System
open System.IO
open System.Collections.Generic
open Utilities
open RepoDomainModel

let rec getFiles (dir:DirectoryInfo) =
    [
        yield! (dir.GetFiles() |> Array.toList)
        yield! (dir.GetDirectories() |> Array.toList |> List.collect getFiles)
    ]

let getFilePath (file:FileInfo) = file.FullName

let hashFile path =
    async {
        use stream = File.OpenRead path
        let! hash = Hashing.getHash stream
        return {Signature=hash ;Path=path}
    }

let createModpack path =
    async {
        let fullPath = Environment.CurrentDirectory ++ path
        let dir = new DirectoryInfo(fullPath)
        let! files =
            dir
            |> getFiles
            |> List.map getFilePath
            |> List.map hashFile
            |> Async.sequenceList

        return {Name=dir.Name;Files=files}
    }