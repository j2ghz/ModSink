using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Anotar.Serilog;
using DynamicData;
using FluentAssertions;
using ModSink.Common.Client;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;
using ReactiveUI.Testing;
using Serilog;
using Xunit;

namespace ModSink.Common.Tests.Client
{
    public class ClientServiceTests : IDisposable
    {
        public ClientServiceTests()
        {
            tempRoot.Create();
        }

        public void Dispose()
        {
            try
            {
                tempRoot.Parent?.Delete(true);
            }
            catch (Exception e)
            {
                throw new Exception("Cleanup of temporary files failed", e);
            }
        }

        private readonly DirectoryInfo tempRoot = new DirectoryInfo(Path.GetTempPath()).ChildDir("ModSink")
            .ChildDir(Guid.NewGuid().ToString());

        [Fact(Skip = "Outdated test")]
        public void DownloadRepo()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            //Arrange
            var schedDisposable = TestUtils.WithScheduler(ImmediateScheduler.Instance);
            var repoUri = new Uri("http://localhost/repo.bin");
            var group = new Group
            {
                RepoInfos = new List<RepoInfo> {new RepoInfo {Uri = repoUri}}
            };
            var formatter = new BinaryFormatter();
            var groupStream = new MemoryStream();
            formatter.Serialize(groupStream, group);
            groupStream.Position = 0;
            var fileUri = new Uri("http://localhost/file.bin");
            var fileSignature = new FileSignature(
                new HashValue(new byte[] {0x99, 0xE9, 0xD8, 0x51, 0x37, 0xDB, 0x46, 0xEF}),
                1);
            var repo = new Repo
            {
                Files = new Dictionary<FileSignature, Uri>
                {
                    {
                        fileSignature,
                        fileUri
                    }
                },
                Modpacks = new List<Modpack>
                {
                    new Modpack
                    {
                        Name = "First",
                        Mods = new List<ModEntry>
                        {
                            new ModEntry
                            {
                                Mod = new Mod
                                {
                                    Name = "FirstM",
                                    Version = "v1",
                                    Files = new Dictionary<Uri, FileSignature>
                                    {
                                        {new Uri("file.bin", UriKind.Relative), fileSignature}
                                    }
                                }
                            }
                        }
                    }
                }
            };
            var repoStream = new MemoryStream();
            formatter.Serialize(repoStream, repo);
            repo.BaseUri = new Uri("http://localhost/");
            repoStream.Position = 0;
            var fileSource = new byte[] {0xFA};
            var fileStream = new MemoryStream(fileSource);

            var groupUri = new Uri("http://localhost/group.bin");
            var downloader = new MockDownloader(new Dictionary<Uri, Stream>
            {
                {groupUri, groupStream},
                {repoUri, repoStream},
                {fileUri, fileStream}
            });

            var downloadsDir = tempRoot.ChildDir("downloads");
            if (!downloadsDir.Exists) downloadsDir.Create();

            var client = new ClientService(downloader, formatter, downloadsDir);
            //Act
            client.GroupUrls.Edit(a => a.AddOrUpdate(groupUri.ToString()));
            //Assert

            client.GroupUrls.Items.Should().HaveCount(1);
            client.Repos.Items.Should().HaveCount(1);
            client.QueuedDownloads.Items.Should().HaveCount(0);
            foreach (var r in client.Repos.Items)
            foreach (var modpack in r.Modpacks)
                modpack.Selected = true;

            client.QueuedDownloads.Items.Should().HaveCount(1);
            client.ActiveDownloads.Items.Should().HaveCount(1);
            var item = client.ActiveDownloads.Items.Single();
            item.Progress.Subscribe();
            var file = downloadsDir.ChildFile(fileSignature.Hash.ToString());
            file.Exists.Should().BeTrue();
            client.GroupUrls.Clear();
            File.ReadAllBytes(file.FullName).Should().BeEquivalentTo(fileSource);
            //Clean up
            client.Dispose();
            schedDisposable.Dispose();
        }
    }
}