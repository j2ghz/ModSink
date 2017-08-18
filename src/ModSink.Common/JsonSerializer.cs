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
        private JsonSerializerSettings serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented };

        public Repo Deserialize(string repo)
        {
            return JsonConvert.DeserializeObject<Repo>(repo, serializerSettings);
        }

        public async Task<Repo> Deserialize(Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                var str = await sr.ReadToEndAsync();
                return Deserialize(str);
            }
        }

        public string Serialize(Repo repo)
        {
            return JsonConvert.SerializeObject(repo, serializerSettings);
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