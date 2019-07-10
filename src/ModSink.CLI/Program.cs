using CommandLine;
using ModSink.CLI.Verbs;

namespace ModSink.CLI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Parser.Default.ParseArguments<Chunk.Options>(args)
            //    .MapResult(
            //        Chunk.Run,
            //        errs => 1
            //    );
            Chunk.Run(new Chunk.Options()
                {Path = @"G:\417addons\@cup_terrains_core\addons\cup_terrains_ca_roads_e.pbo", Zeroes = 20});
        }
    }
}