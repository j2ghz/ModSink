using System;
using System.IO.Abstractions;
using ModSink.Application;
using ModSink.Application.Hashing;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Infrastructure.RepoBuilders
{
    public class ModsInRepoRootBuilder : IRepoBuilder
    {
        private readonly IHashingService _hashingService;

        public ModsInRepoRootBuilder(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public Repo Build(IDirectoryInfo root)
        {
            //Read config
            //hash requested mods
            //build modpacks
            //build repo
            throw new NotImplementedException();
        }
    }
}