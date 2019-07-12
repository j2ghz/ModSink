using CommandLine;
using ModSink.CLI.Verbs;

namespace ModSink.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Chunk.Options>(args)
                .MapResult(
                    Chunk.Run,
                    errs => 1
                );
        }
    }
}