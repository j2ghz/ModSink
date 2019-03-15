using System;
using System.Data;
using DynamicData;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using ModSink.Common.Models.Repo;
using ModSink.UI.ViewModel;
using Xunit;

namespace ModSink.UI.Tests.ViewModel
{
    public class LibraryViewModelTests
    {
        [Fact(Skip = "Outdated")]
        public void AddModpack()
        {
            ReactiveUI.Testing.TestUtils.With(new TestScheduler(), scheduler =>
            {
                var cache = new SourceCache<Modpack,Guid>(m=>m.Id);
                var vm = new LibraryViewModel(cache);
                vm.Modpacks.Should().BeEmpty();
                cache.AddOrUpdate(new Modpack());
                scheduler.AdvanceBy(1);
                vm.Modpacks.Should().HaveCount(1);
            });
        }
    }
}