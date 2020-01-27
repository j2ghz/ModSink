using System.IO;
using System.IO.Abstractions;

namespace ModSink.Application.Hashing {
  public interface IFileOpener { Stream OpenRead(IFileInfo file); }
}
