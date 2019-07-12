using CommandLine;
using Humanizer;
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
                {Path = @"E:\SteamLibrary\steamapps\common\HITMAN2\Runtime\dlc4patch1.rpkg", FileStreamBuffer = 10*1024*1024});
        }
    }
}