open System
open Argu
open System.Diagnostics
open System.Reflection
open Serilog
open Serilog.Events
open Newtonsoft.Json
open System.IO

type ModSinkArgs =
    | Version
    | [<AltCommandLine("-v")>] Verbose
    | [<CliPrefix(CliPrefix.None)>] Create of ParseResults<CreateArgs>
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Version _ -> "Prints the current installed version"
            | Verbose _ -> "Set verbose output"
            | Create _ -> "Create a repo"

and CreateArgs =
    | [<MainCommand;ExactlyOnce;Last>] Repo of path: string
    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Repo _ -> "Paths to folder with all modpacks to include"

[<EntryPoint>]
let main argv =
    printfn "ModSink %s" 
        (FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion)
    let parser = ArgumentParser.Create<ModSinkArgs>(programName = "ModSink.exe")
    let parsed = parser.Parse(argv)
    if parsed.IsUsageRequested || parsed.GetAllResults() |> List.isEmpty then 
        parsed.Raise "Invalid usage" true
    else 
        let level =
            if parsed.Contains Verbose then LogEventLevel.Verbose
            else LogEventLevel.Information
        do Log.Logger <- ((new LoggerConfiguration()).Enrich.FromLogContext()
            .WriteTo.Console(level).CreateLogger() :> ILogger)
    match parsed.GetSubCommand() with
    | Create a -> 
        a.GetResult Repo
        |> RepoUtilities.create
        |> Async.RunSynchronously
        |> JsonConvert.SerializeObject
        |> fun str -> File.WriteAllText("repo.json",str)
        do Log.Information("Saved at {path}", Path.GetFullPath("repo.json"))
    | a -> 
        a
        |> sprintf "%O"
        |> failwith
    0
