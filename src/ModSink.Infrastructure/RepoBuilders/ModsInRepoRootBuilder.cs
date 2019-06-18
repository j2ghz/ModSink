using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using ModSink.Application;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder
    {
        public ModsInRepoRootBuilder()
        {
            
        }

        public Repo Build(IDirectoryInfo root)
        {
            var repo = new Repo();
            var modpacks = root.EnumerateDirectories()
                .Select(BuildMod);

        }

        private Mod BuildMod(IDirectoryInfo root)
        {
            var mod = new Mod()
            {
                Name=root.Name,
                Files = { }
            };
        }
    }
}
