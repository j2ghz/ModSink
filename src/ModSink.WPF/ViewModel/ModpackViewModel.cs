using System.Linq;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Common.Models.Repo;
using ReactiveUI;

namespace ModSink.WPF.ViewModel
{
    public class ModpackViewModel : ReactiveObject
    {
        public ModpackViewModel(Modpack modpack)
        {
            Modpack = modpack;
            Size = ByteSize.FromBytes(
                Modpack.Mods
                    .SelectMany(m => m.Mod.Files)
                    .Select(f => f.Value.Length)
                    .Aggregate((sum, a) => sum + a)).Humanize("G03");
        }

        public Modpack Modpack { get; }

        public ReactiveCommand Download { get; }

        public string Size { get; }
    }
}