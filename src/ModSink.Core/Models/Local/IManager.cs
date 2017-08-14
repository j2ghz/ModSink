﻿using ModSink.Core.Models.Remote.Group;
using ModSink.Core.Models.Remote.Repo;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace ModSink.Core.Models.Local
{
    public interface IManager
    {
        Task DownloadMissingFiles(Modpack modpack);

        IObservable<IHashValue> GetMissingFiles(Modpack modpack);

        IObservable<Modpack> GetModpacks();
    }
}