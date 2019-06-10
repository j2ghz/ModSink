using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Humanizer.Bytes;
using ModSink.Application;
using ModSink.Domain.Entities;
using Multiformats.Hash;

namespace ModSink.Infrastructure.Downloaders.Http
{
    public class HttpClientDownloader : IDownloader
    {
        private readonly HttpClient _client = new HttpClient();

        public HttpClientDownloader()
        {
        }

        public bool CanDownload(Uri uri)
        {
            //Check for absolute Uri
            if (!uri.IsAbsoluteUri)
            {
                if (_client.BaseAddress?.IsAbsoluteUri == true)
                    uri = new Uri(_client.BaseAddress, uri);
                else
                    return false;
            }
            //Check for HTTP
            return uri.Scheme == "http" || uri.Scheme == "https";
        }

        public async Task<Stream> DownloadAsync(Uri uri,
            CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }
        
    }
}