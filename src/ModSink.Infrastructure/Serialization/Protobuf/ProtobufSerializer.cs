using System;
using System.IO;
using System.Linq;
using AutoMapper;
using Google.Protobuf;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;
using PathLib;

namespace ModSink.Infrastructure.Serialization.Protobuf
{
    public class ProtobufSerializer : IFormatter
    {
        private readonly Mapper mapper;

        public ProtobufSerializer()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Repo, Model.Repo>()
                .ForMember(r => r.Name, c => c.MapFrom(r => r.Name))
                .ForMember(r => r.ChunksPath, c => c.MapFrom(r => r.ChunksPath))
                .ForMember(r => r.Modpacks, c => c.MapFrom(r => r.Modpacks));
                cfg.CreateMap<Modpack, Model.Modpack>();
                cfg.CreateMap<Mod, Model.Mod>();
                cfg.CreateMap<RelativePathFile, Model.RelativePathFile>();
                cfg.CreateMap<IPurePath, Model.RelativePath>().ForMember(p => p.SerializedRelativeUri, p => p.MapFrom(x => x.ToUri().ToSerializableString()));
                cfg.CreateMap<Signature, Model.Signature>();
                cfg.CreateMap<Hash, Model.Hash>();
                cfg.CreateMap<byte[], ByteString>();

                cfg.CreateMap<Model.Repo, Repo>().ConstructUsing((r, ctx) => new Repo(r.Name,
                    r.Modpacks.Select(m => ctx.Mapper.Map<Modpack>(m)).ToList(), r.ChunksPath));
            });
            mapperConfig.CompileMappings();
            mapperConfig.AssertConfigurationIsValid();
            mapper = new Mapper(mapperConfig);
        }

        private Model.Repo Map(Repo repo)
        {
            var nRepo = new Model.Repo()
            {
                Name = repo.Name,
                ChunksPath = repo.ChunksPath

            };
            nRepo.Modpacks.AddRange(repo.Modpacks.Select(modpack =>
                {
                    var nModpack = new Model.Modpack { Name = modpack.Name };
                    nModpack.Mods.AddRange(modpack.Mods.Select(mod =>
                    {
                        var nMod = new Model.Mod() { Name = mod.Name };
                        nMod.Files.AddRange(mod.Files.Select(f => new Model.RelativePathFile() { RelativePath = new Model.RelativePath() { SerializedRelativeUri = f.RelativePath.ToUri().ToSerializableString() }, Signature = new Model.Signature() { Length = f.Signature.Length, Hash = new Model.Hash() { Value = ByteString.CopyFrom(f.Signature.Hash.Value), HashId = f.Signature.Hash.HashId } } }));
                        return nMod;
                    }));
                    return nModpack;
                }));
            return nRepo;
        }

        public FileChunks DeserializeFileChunks(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Repo DeserializeRepo(Stream stream)
        {
            var intermediate = Model.Repo.Parser.ParseFrom(stream);
            return mapper.Map<Repo>(intermediate);
        }

        public Repo MapAndBack(Repo repo)
        {
            var mapped = mapper.Map<Model.Repo>(repo);
            return mapper.Map<Repo>(mapped);
        }

        public Stream SerializeFileChunks(FileChunks fileChunks)
        {
            throw new NotImplementedException();
        }

        public Stream SerializeRepo(Repo repo)
        {
            var result = new MemoryStream();
            var intermediate = mapper.Map<Model.Repo>(repo);
            using var cos = new CodedOutputStream(result, true);
            intermediate.WriteTo(cos);
            return result;
        }
    }
}