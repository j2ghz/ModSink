// Learn more about F# at http://fsharp.org

open System
open Argu
open System.Diagnostics
open System.Reflection

[<EntryPoint>]
let main argv =
    printfn "ModSink %s" (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion)
    //try
    match argv |> List.ofArray with
    | "create" :: args ->
        match args with
        | [path] -> RepoUtilities.create path
        | _ -> printfn "Usage: modsink create [path]"
    | _ ->
        printfn "Usage: modsink <create|dump>"
    0
    //with e ->
    //    ExceptionExtentions.ToStringDemystified e |> eprintfn "%s"
    //    Debugger.Break()
    //    e.HResult