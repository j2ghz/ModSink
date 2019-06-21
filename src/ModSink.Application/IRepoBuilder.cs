﻿using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Domain.Entities.Repo;

namespace ModSink.Application
{
    public interface IRepoBuilder
    {
        Repo Build(System.IO.Abstractions.IDirectoryInfo root);
    }
}
