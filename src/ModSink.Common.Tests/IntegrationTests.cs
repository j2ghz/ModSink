using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DynamicData;
using FluentAssertions;
using Modsink.Common.Tests;
using ModSink.Common.Models.Group;
using ModSink.Common.Models.Repo;
using ReactiveUI;
using ReactiveUI.Testing;
using Serilog;
using Xunit;

namespace ModSink.Common.Tests
{
    public class IntegrationTests : IDisposable
    {
        public IntegrationTests()
        {
            tempRoot.Create();
        }

        public void Dispose()
        {
            tempRoot.Parent?.Delete(true);
        }

        private readonly DirectoryInfo tempRoot = new DirectoryInfo(Path.GetTempPath()).ChildDir("ModSink")
            .ChildDir(Guid.NewGuid().ToString());

        [Fact]
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
            var fileStream = new MemoryStream(new byte[] {0xFA});

            var groupUri = new Uri("http://localhost/group.bin");
            var downloader = new MockDownloader(new Dictionary<Uri, Stream>
            {
                {new Uri(groupUri, "./test"), groupStream},
                {repoUri, repoStream},
                {fileUri, fileStream}
            });

            var m = new ModSinkBuilder()
                .WithDownloader(downloader)
                .WithFormatter(formatter)
                .InDirectory(tempRoot.ChildDir("downloads"), tempRoot.ChildDir("temp")).Build();
            //Act
            m.Client.GroupUrls.Edit(a => a.Add(groupUri.ToString()));
            //Assert

            //m.Client.GroupUrls.Items.Should().HaveCount(1);
            //m.Client.Repos.Items.Should().HaveCount(1);
            //m.Client.Repos.Items.Should().AllBeEquivalentTo(repo);
            //m.Client.DownloadService.QueuedDownloads.Items.Should().HaveCount(1);
            //Clean up
            schedDisposable.Dispose();
        }
    }
}