using CommandLine;
using ModSink.CLI.Verbs;

namespace ModSink.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Chunk.Options,DuplicateChunks.Options>(args)
                .MapResult(
                    new Chunk().Run,
                    new DuplicateChunks().Run,
                    errs => 1
                );
        }
    }
}