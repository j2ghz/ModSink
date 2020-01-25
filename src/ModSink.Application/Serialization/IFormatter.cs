using System.IO;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Serialization {
  public interface IFormatter {
    FileChunks DeserializeFileChunks(Stream stream);
    Domain.Entities.Repo.Repo DeserializeRepo(Stream stream);
    Stream SerializeFileChunks(FileChunks fileChunks);
    Stream SerializeRepo(Domain.Entities.Repo.Repo repo);
    Domain.Entities.Repo.Repo MapAndBack(Domain.Entities.Repo.Repo repo);
  }

  public abstract class GenericFormatter : IFormatter {

    public FileChunks DeserializeFileChunks(Stream stream) {
      return Deserialize<FileChunks>(stream);
    }

    public Domain.Entities.Repo.Repo DeserializeRepo(Stream stream) {
      return Deserialize<Domain.Entities.Repo.Repo>(stream);
    }

    public Stream SerializeFileChunks(FileChunks fileChunks) {
      return Serialize(fileChunks);
    }

    public Stream SerializeRepo(Domain.Entities.Repo.Repo repo) {
      return Serialize(repo);
    }

    public abstract T Deserialize<T>(Stream stream);
    public abstract Stream Serialize<T>(T o);

    public Domain.Entities.Repo.Repo
    MapAndBack(Domain.Entities.Repo.Repo repo) {
      throw new System.NotImplementedException();
    }
  }
}