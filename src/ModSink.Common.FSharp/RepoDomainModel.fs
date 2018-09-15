[<AutoOpen>]
module RepoDomainModel

open System

type HashValue = byte seq

type FileSignature =
    { HashValue : HashValue
      Length : uint64 }

type RelativePath = string

type RelativeFile =
    { Signature : FileSignature
      Path : RelativePath }

type Mod =
    { Name : string
      Files : RelativeFile seq }

type ModEntry =
    { Mod : Mod
      Default : bool
      Required : bool }

type Modpack =
    { Name : string
      Mods : ModEntry seq
      Selected : IObservable<bool> }

type Repo =
    { Modpacks : Modpack seq
      Files : RelativeFile seq
      BaseUri : Uri }
