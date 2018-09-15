module RepoUtilities

open System
open DynamicData
open DynamicDataEx

let getFile (repo : Repo) = repo.Files
let getFileSignature (file : RelativeFile) = file.Signature
let getOnlineFiles (repos : IObservable<IChangeSet<Repo, Uri>>) =
    repos |> transformMany getFile getFileSignature

let getDownloads (modpacks : IObservable<IChangeSet<Modpack, Guid>>) =
    modpacks
    //  |> autoRefresh (fun modpack -> modpack.Selected)
    |> filter (``return`` (fun modpack -> modpack.Selected)) ()
    |> transformMany (fun modpack -> modpack.Mods) (fun _ -> Guid.NewGuid())
    |> transformMany (fun modentry -> modentry.Mod.Files) 
           (fun file -> file.Signature)
