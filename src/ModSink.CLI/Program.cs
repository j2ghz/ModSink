using CommandLine;
using ModSink.CLI.Verbs;

namespace ModSink.CLI {
  public class Program {
    public static void Main(string[] args) {
      Parser.Default
          .ParseArguments<Chunk.Options, DuplicateChunks.Options>(args)
          .MapResult((Chunk.Options o) => new Chunk().Run(o),
                     (DuplicateChunks.Options o) =>
                         new DuplicateChunks().Run(o),
                     errs => 1);
    }
  }
}
