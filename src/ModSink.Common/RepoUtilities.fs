module RepoUtilities

open Microsoft.FSharpLu.File
open System
open System.IO
open System.Collections.Generic
open Utilities
open RepoDomainModel
open Serilog
open FSharp.Control
open Microsoft.FSharpLu

let rec getFiles dir =
    seq {
        yield! (dir |> Directory.EnumerateFiles)
        yield! (dir
                |> Directory.EnumerateDirectories
                |> Seq.collect getFiles)
    }

let getFilePath(file: FileInfo) = file.FullName
let getDirectoryName path = (new DirectoryInfo(path)).Name

let hashFile root (path: string) =
    async {
        do Log.Verbose("Hashing {path}",path)
        use stream = File.OpenRead path
        let startTime = DateTime.Now
        let! hash = Hashing.getHash stream
        do Log.Information("Hashed {path}",path)
        return {Signature = hash
                Path = path.Substring(String.length root)}
    }

let createMod root (path: string) =
    async {
        do Log.Information("Creating mod at {path}",path)
        let! files = path
                     |> getFiles
                     |> AsyncSeq.ofSeq
                     |> AsyncSeq.mapAsync (hashFile root)
                     |> AsyncSeq.toListAsync
        return {Name = (getDirectoryName path)
                Files = files}
    }

let createModpack root (path: string) =
    async {
        do Log.Information("Creating modpack at {path}",path)
        let! mods = path
                    |> Directory.EnumerateDirectories
                    |> AsyncSeq.ofSeq
                    |> AsyncSeq.mapAsync (createMod root)
                    |> AsyncSeq.map(function 
                           | (m) -> 
                               {Mod = m
                                Default = true
                                Required = true})
                    |> AsyncSeq.toListAsync
        return {Name = (getDirectoryName path)
                Mods = mods}
    }

let create (path: string) =
    async {
        let! mp =
            path
            |> Directory.EnumerateDirectories
            |> AsyncSeq.ofSeq
            |> AsyncSeq.mapAsync (createModpack path)
            |> AsyncSeq.toListAsync
        return {Modpacks = mp}
    }
