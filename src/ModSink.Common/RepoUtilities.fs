module RepoUtilities

open Microsoft.FSharpLu.File
open System
open System.IO
open System.Collections.Generic
open Utilities
open RepoDomainModel
open Serilog
open FSharp.Control

let rec getFiles dir =
    seq { 
        yield! (dir |> Directory.EnumerateFiles)
        yield! (dir
                |> Directory.EnumerateDirectories
                |> Seq.collect getFiles)
    }

let getFilePath (file : FileInfo) = file.FullName
let getDirectoryName path = (new DirectoryInfo(path)).Name

let hashFile path =
    async { 
        use stream = File.OpenRead path
        let! hash = Hashing.getHash stream
        return { Signature = hash
                 Path = path }
    }

let createMod path =
    async { 
        let! files = path
                     |> getFiles
                     |> Seq.map hashFile
                     |> Seq.asyncSequence
        let fileList = List.ofSeq files
        return { Name = (getDirectoryName path)
                 Files = fileList }
    }

let createModpack (path : string) =
    async { 
        do Log.Information("Creating modpack at {path}", path)
        AsyncSeq
        let! mods = path
                    |> Directory.EnumerateDirectories
                    |> Seq.map createMod
                    |> AsyncSeq.
        let modEntries =
            mods
            |> List.ofSeq
            |> List.map (function 
                   | (m) -> 
                       { Mod = m
                         Default = true
                         Required = true })
        return { Name = (getDirectoryName path)
                 Mods = modEntries }
    }

let create (paths : string list) =
    paths
    |> List.map createModpack
    |> List.asyncSequence
