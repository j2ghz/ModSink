module RepoUtilities
open Microsoft.FSharpLu.File
open System
open System.IO
open System.Collections.Generic

let rec getFiles (dir:DirectoryInfo) =
    dir.GetFiles() :: (dir.GetDirectories() |> Array.toList |> List.collect getFiles)

let hashFiles filePaths =
        filePaths
        |> List.map (fun path ->
            async {
                use stream = File.OpenRead path
                let! hash = Hashing.getHash stream
                return {Signature=hash ;Path=path}
            })

let createMod path =
    Utilities.optional {
        let! fullPath = getExistingDir (Environment.CurrentDirectory ++ path)
        let dir = new DirectoryInfo(fullPath)
        let mods =
            dir.EnumerateDirectories()
            |> Seq.map (fun d -> d)
        return 0
    }