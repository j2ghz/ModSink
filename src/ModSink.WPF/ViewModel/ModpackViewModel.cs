using System.Linq;
using System.Threading.Tasks;
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
            Install = ReactiveCommand.CreateFromTask(() => Task.Run(() => Modpack.Selected = true),
                outputScheduler: RxApp.TaskpoolScheduler);
        }

        public ReactiveCommand Install { get; }

        public Modpack Modpack { get; }

        public string Size { get; }
    }
}