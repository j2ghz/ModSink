using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using Bogus;
using DynamicData;
using FluentAssertions;
using ModSink.Common.Client;
using ModSink.Common.Models.DTO.Repo;
using ReactiveUI.Testing;
using Xunit;

namespace ModSink.Common.Tests.Client
{
    public class DynamicDataChainTests
    {
        [Fact]
        public void GetDownloadsFromModpacks()
        {
            using (TestUtils.WithScheduler(ImmediateScheduler.Instance))
            {
                var modpacks = new SourceCache<Modpack, Guid>(_ => new Guid());
                var files = DynamicDataChain.GetDownloadsFromModpacks(modpacks.Connect()).AsObservableCache();
                files.Count.Should().Be(0);
                var modpack = new Modpack
                {
                    Mods = new List<ModEntry>
                    {
                        new ModEntry
                        {
                            Mod = new Mod
                            {
                                Files = new Dictionary<Uri, FileSignature>
                                {
                                    {new Uri("http://a.b/"), new FileSignature(new HashValue(new byte[] {0x00}), 0)}
                                }
                            }
                        }
                    }
                };
                modpacks.AddOrUpdate(modpack);
                files.Count.Should().Be(0);
                modpack.Selected = true;
                files.Count.Should().Be(1);
                modpack.Selected = false;
                files.Count.Should().Be(0);
            }
        }

        [Fact]
        public void GetModpacksFromReposTest()
        {
            using (TestUtils.WithScheduler(ImmediateScheduler.Instance))
            {
                var faker = new Faker();
                var repos = new SourceCache<Repo, Uri>(r => r.BaseUri);
                var modpacks = DynamicDataChain.GetModpacksFromRepos(repos.Connect()).AsObservableCache();
                modpacks.Count.Should().Be(0);
                repos.AddOrUpdate(
                    new Repo
                    {
                        BaseUri = new Uri(faker.Internet.UrlWithPath()),
                        Modpacks = new List<Modpack> {new Modpack(), new Modpack()}
                    });
                modpacks.Count.Should().Be(2);
                repos.Clear();
                modpacks.Count.Should().Be(0);
            }
        }

        [Fact]
        public void ObservableCacheRemoveKeyClears()
        {
            var source = new SourceCache<string, string>(i => i);
            var dest = source.Connect().RemoveKey().AsObservableList();
            source.AddOrUpdate("");
            source.Items.Should().HaveCount(1);
            source.Clear();
            source.Items.Should().HaveCount(0);
            dest.Items.Should().HaveCount(0);
        }

        [Fact]
        public void ObservableCacheRemoveKeyTransformClears()
        {
            var source = new SourceCache<string, string>(i => i);
            var dest = source.Connect().TransformMany(i => Enumerable.Repeat("", 1), i => i).AsObservableCache();
            source.AddOrUpdate("");
            source.Items.Should().HaveCount(1);
            dest.Items.Should().HaveCount(1);
            source.Clear();
            source.Items.Should().HaveCount(0);
            dest.Items.Should().HaveCount(0);
        }

        [Fact]
        public void ObservableListClears()
        {
            var source = new SourceList<string>();
            var dest = source.AsObservableList();
            source.Add("");
            source.Clear();
            source.Items.Should().HaveCount(0);
            dest.Items.Should().HaveCount(0);
        }

        [Fact]
        public void ObservableListConnectClears()
        {
            var source = new SourceList<string>();
            var dest = source.Connect().AsObservableList();
            source.Add("");
            source.Clear();
            source.Items.Should().HaveCount(0);
            dest.Items.Should().HaveCount(0);
        }

        [Fact]
        public void SourceListClears()
        {
            var source = new SourceList<string>();
            source.Add("");
            source.Clear();
            source.Items.Should().HaveCount(0);
        }
    }
}