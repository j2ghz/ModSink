using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.Serialization.Protobuf
{
    public class ProtobufSerializer : IFormatter
    {
        public bool CanDeserialize(string extension)
        {
            throw new NotImplementedException();
        }

        public FileChunks DeserializeFileChunks(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Repo DeserializeRepo(Stream stream)
        {
            throw new NotImplementedException();
        }

        public Stream SerializeFileChunks(FileChunks fileChunks)
        {
            throw new NotImplementedException();
        }

        public Stream SerializeRepo(Repo repoRoot)
        {
            throw new NotImplementedException();
        }
    }
}
