using System.Linq;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Core.Models.Repo;

namespace ModSink.WPF.ViewModel
{
    public class ModpackViewModel
    {
        public ModpackViewModel(Modpack modpack, Repo repo)
        {
            Modpack = modpack;
            Size = ByteSize.FromBytes(
                Modpack.Mods
                    .SelectMany(m => m.Mod.Files)
                    .Select(f => f.Value.Length)
                    .Aggregate((sum, a) => sum + a)).Humanize("G03");
            Repo = repo.BaseUri.ToString();
        }
        public string Repo { get; }
        public Modpack Modpack { get; }

        public string Size { get; }
    }
}