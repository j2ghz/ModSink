module Model

type RelativePath = RelativePath of string

type Hash =
    { HashId: string
      Value: byte list }

type Signature =
    { Hash: Hash
      Length: int64 }

type Chunk =
    { Signature: Signature
      Position: int64 }

type FileChunks =
    { File: Signature
      Chunks: Chunk list }

type Mod =
    { Name: string
      Files: Map<RelativePath, Signature> }

type Modpack =
    { Name: string
      Mods: Mod Set }

type Repo =
    { Name: string
      Modpacks: Modpack Set
      Files: Map<Signature, RelativePath> }
