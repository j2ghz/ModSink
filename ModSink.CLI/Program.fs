// Learn more about F# at http://fsharp.org

open System
open Argu
open System.Diagnostics

[<CliPrefix(CliPrefix.Dash)>]
type CleanArgs =
    | D
    | F
    | X
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | D -> "Remove untracked directories in addition to untracked files"
            | F -> "Git clean will refuse to delete files or directories unless given -f."
            | X -> "Remove only files ignored by Git."
and CommitArgs =
    | Amend
    | [<AltCommandLine("-p")>] Patch
    | [<AltCommandLine("-m")>] Message of msg:string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Amend -> "Replace the tip of the current branch by creating a new commit."
            | Patch -> "Use the interactive patch selection interface to chose which changes to commit."
            | Message _ -> "Use the given <msg> as the commit message. "
and CreateArgs =
    | Name of string
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Name _ -> "Name of the repo to be created."
and ModSinkArgs =
    | Version
    | [<AltCommandLine("-v")>] Verbose
    | [<CliPrefix(CliPrefix.None)>] Create of ParseResults<CreateArgs>
    | [<CliPrefix(CliPrefix.None)>] Dump of ParseResults<CleanArgs>
    | [<CliPrefix(CliPrefix.None)>] Check of ParseResults<CleanArgs>
    | [<CliPrefix(CliPrefix.None)>] Import of ParseResults<CleanArgs>
    | [<CliPrefix(CliPrefix.None)>] Clean of ParseResults<CleanArgs>
    | [<CliPrefix(CliPrefix.None)>] Collision_Check of ParseResults<CleanArgs>
with
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Prints the Git suite version that the git program came from."
            | Verbose -> "Print a lot of output to stdout."
            | Clean _ -> "Remove untracked files from the working tree."

[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<ModSinkArgs>("modsink","text",checkStructure=true)
        parser.PrintUsage() |> printf "%s"
        0
    with e ->
        ExceptionExtentions.ToStringDemystified e |> eprintfn "%s"
        e.HResult