using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Google.Protobuf;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;

namespace ModSink.Infrastructure.Serialization.Protobuf
{
    public class ProtobufSerializer : IFormatter
    {
        public FileChunks DeserializeFileChunks(Stream stream) => throw new NotImplementedException();

        public Repo DeserializeRepo(Stream stream)
        {
            var intermediate = Model.Repo.Parser.ParseFrom(stream);
            return MapBack(intermediate); //mapper.Map<Repo>(intermediate);
        }

        public Repo MapAndBack(Repo repo)
        {
            //var mapped = mapper.Map<Model.Repo>(repo);
            //return mapper.Map<Repo>(mapped);
            var mapped = Map(repo);
            return MapBack(mapped);
        }

        public Stream SerializeFileChunks(FileChunks fileChunks) => throw new NotImplementedException();

        public Stream SerializeRepo(Repo repo)
        {
            var result = new MemoryStream();
            var intermediate = Map(repo); //mapper.Map<Model.Repo>(repo);
            using var cos = new CodedOutputStream(result, true);
            intermediate.WriteTo(cos);
            return result;
        }

        private Model.Repo Map(Repo repo)
        {
            var nRepo = new Model.Repo { Name = repo.Name, ChunksPath = repo.ChunksPath };
            nRepo.Modpacks.AddRange(repo.Modpacks.Select(modpack =>
            {
                var nModpack = new Model.Modpack { Name = modpack.Name };
                nModpack.Mods.AddRange(modpack.Mods.Select(mod =>
                {
                    var nMod = new Model.Mod { Name = mod.Name };
                    nMod.Files.AddRange(mod.Files.Select(f => new Model.RelativePathFile
                    {
                        RelativePath =
                            TypeDescriptor.GetConverter(typeof(IPurePath)).ConvertToInvariantString(f.RelativePath),
                        Signature = new Model.Signature
                        {
                            Length = f.Signature.Length,
                            Hash = new Model.Hash
                            {
                                Value = ByteString.CopyFrom(f.Signature.Hash.Value),
                                HashId = f.Signature.Hash.HashId
                            }
                        }
                    }));
                    return nMod;
                }));
                return nModpack;
            }));
            return nRepo;
        }

        private Repo MapBack(Model.Repo repo) =>
            new Repo(repo.Name,
                repo.Modpacks.Select(mp => new Modpack
                {
                    Name = mp.Name,
                    Mods = mp.Mods.Select(m => new Mod
                    {
                        Name = m.Name,
                        Files = m.Files.Select(f => new RelativePathFile
                        {
                            RelativePath =
                                (IPurePath)TypeDescriptor.GetConverter(typeof(IPurePath))
                                    .ConvertFromInvariantString(f.RelativePath),
                            Signature = new Signature(
                                new Hash(f.Signature.Hash.HashId, f.Signature.Hash.Value.ToByteArray()),
                                f.Signature.Length)
                        }).ToList()
                    }).ToList()
                }).ToList(),
                repo.ChunksPath);
    }
}
