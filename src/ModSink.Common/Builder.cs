using System;
using System.Collections.Generic;
using System.Text;
using ModSink.Core.Models;
using System.Composition.Hosting;
using System.Reflection;

namespace ModSink.Common
{
    public class Builder
    {
        private ContainerConfiguration config = new ContainerConfiguration().WithAssembly(typeof(Builder).GetTypeInfo().Assembly);

        public IModSink Build()
        {
            var container = config.CreateContainer();

            return container.GetExport<IModSink>();
        }
    }
}