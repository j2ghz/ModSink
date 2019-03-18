using System;
using DynamicData;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using ModSink.Common.Models.Client;
using ModSink.UI.ViewModel;
using ReactiveUI.Testing;
using Xunit;

namespace ModSink.UI.Tests.ViewModel
{
    public class LibraryViewModelTests
    {
        [Fact(Skip = "Outdated")]
        public void AddModpack()
        {
            new TestScheduler().With(scheduler =>
            {
                var cache = new SourceCache<Modpack, Guid>(_ => Guid.NewGuid());
                var vm = new LibraryViewModel(cache);
                vm.Modpacks.Should().BeEmpty();
                cache.AddOrUpdate(new Modpack(new Common.Models.DTO.Repo.Modpack()));
                scheduler.AdvanceBy(1);
                vm.Modpacks.Should().HaveCount(1);
            });
        }
    }
}