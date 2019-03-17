using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Common.Models.DTO.Repo;
using ReactiveUI;
using Modpack = ModSink.Common.Models.Client.Modpack;

namespace ModSink.UI.ViewModel
{
    public class ModpackViewModel : ReactiveObject
    {
        public ModpackViewModel(Common.Models.Client.Modpack modpack)
        {
            Modpack = modpack;
            Size = ByteSize.FromBytes(
                Modpack.Mods
                    .SelectMany(m => m.Mod.Files)
                    .Select(f => f.Value.Length)
                    .Aggregate((sum, a) => sum + a)).Humanize("G03");
        }

        public ReactiveCommand<Unit, bool> Install { get; }

        public Modpack Modpack { get; }

        public string Size { get; }
    }
}