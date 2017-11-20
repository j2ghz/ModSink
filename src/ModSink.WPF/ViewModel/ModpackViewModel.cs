using System;
using System.Linq;
using Humanizer.Bytes;
using ModSink.Core.Models.Repo;

namespace ModSink.WPF.ViewModel
{
    public class ModpackViewModel : Modpack
    {
        public ModpackViewModel()
        {
            Size = new Lazy<ByteSize>(() =>
                ByteSize.FromBytes(Mods.SelectMany(m => m.Mod.Files).Select(f => f.Value.Length)
                    .Aggregate((sum, a) => sum + a)));
        }

        public Lazy<ByteSize> Size { get; }
    }
}