[<AutoOpen>]
module RepoDomainModel

open System

type HashValue = byte seq

type FileSignature =
    { HashValue : HashValue
      Length : int64 }

type RelativePath = string

type RelativeFile =
    { Signature : FileSignature
      Path : RelativePath }

type Mod =
    { Name : string
      Files : RelativeFile list }

type ModEntry =
    { Mod : Mod
      Default : bool
      Required : bool }

type Modpack =
    { Name : string
      Mods : ModEntry list
      Selected : IObservable<bool> }

type Repo =
    { Modpacks : Modpack list
      Files : RelativeFile list
      BaseUri : Uri }