module RepoUtilitiesTests

open RepoUtilities
open Xunit
open FsUnit.Xunit
open System

[<Fact>]
let ``Returns directory name from path``() =
    getDirectoryName "C:\\dir\\" |> should equal "dir"

[<Fact>]
let ``Returns list of files in subdirectories``() =
    getFiles Environment.CurrentDirectory |> Seq.length |> should greaterThan 0