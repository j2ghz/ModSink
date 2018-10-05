module RepoUtilities
open Microsoft.FSharpLu.File
open System
open System.IO
open System.Collections.Generic
open Utilities
open RepoDomainModel

let rec getFiles dir =
    seq {
        yield! (dir |> Directory.EnumerateFiles)
        yield! (dir |> Directory.EnumerateDirectories |> Seq.collect getFiles)
    }

let getFilePath (file:FileInfo) = file.FullName

let getDirectoryName path = (new DirectoryInfo(path)).Name

let hashFile path =
    async {
        use stream = File.OpenRead path
        let! hash = Hashing.getHash stream
        return {Signature=hash ;Path=path}
    }

let createMod path =
    async {
        let! files =
            path
            |> getFiles
            |> Seq.map hashFile
            |> Seq.asyncSequence
        let fileList = List.ofSeq files

        return {Name=(getDirectoryName path);Files=fileList}
    }

let createModpack path =
    async {
        let! mods =
            path
            |> Directory.EnumerateDirectories
            |> Seq.map createMod
            |> Seq.asyncSequence

        let modEntries =
            mods
            |> List.ofSeq
            |> List.map (function (m) -> {Mod=m; Default=true; Required=true})

        return {Name=(getDirectoryName path);Mods=modEntries}
    }