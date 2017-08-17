using ModSink.Core.Models;
using ModSink.Core.Models.Repo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ModSink.Common
{
    public class JsonSerializer : ISerializer
    {
        public string Serialize(Repo repo)
        {
            return JsonConvert.SerializeObject(repo, Formatting.Indented, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        }

        public async Task Serialize(Repo repo, Stream stream)
        {
            using (var sw = new StreamWriter(stream))
            {
                await sw.WriteAsync(Serialize(repo));
            }
        }
    }
}