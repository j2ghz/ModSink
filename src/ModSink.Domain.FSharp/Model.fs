module Model

open PathLib

type RelativePath = RelativePath of p:IPurePath

let c (p:IPurePath) = 
    match p.IsAbsolute() with
    | true -> Error ""
    | false -> Ok (RelativePath p)

type FileSignature = {
    Hash: Hash;
    Length: int64;
}

type Mod = {
    Name: string;
    Files: Map<RelativePath,FileSignature>
}

type Modpack = {
    Name: string;
    Mods: Mod Set;
}

type Hash = {
    Algo: string;
    Value: byte list
}

type Repo = {
    Name: string;
    Modpacks: Modpack Set;
    Files: Map<FileSignature,IPurePath>;
}