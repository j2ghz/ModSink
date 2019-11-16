using System;
using System.IO;
using AutoMapper;
using Google.Protobuf;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.Serialization.Protobuf
{
    public class ProtobufSerializer : IFormatter
    {
        private readonly Mapper mapper;

        public ProtobufSerializer()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Repo, Model.Repo>();
                cfg.CreateMap<Modpack, Model.Modpack>();
                cfg.CreateMap<Mod, Model.Mod>();
                cfg.CreateMap<RelativePathFile, Model.RelativePathFileSignature>();
                //cfg.CreateMap<Model.Repo, Repo>();
            });
            mapperConfig.CompileMappings();
            mapperConfig.AssertConfigurationIsValid();
            mapper = new Mapper(mapperConfig);
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

        public Stream SerializeFileChunks(FileChunks fileChunks)
        {
            throw new NotImplementedException();
        }

        public Stream SerializeRepo(Repo repo)
        {
            var result = new MemoryStream();
            var intermediate = mapper.Map<Model.Repo>(repo);
            intermediate.WriteTo(new CodedOutputStream(result, true));
            return result;
        }
    }
}