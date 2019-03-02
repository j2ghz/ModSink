using System;
using DynamicData;
using FluentAssertions;
using ModSink.Common.Models.Repo;
using ModSink.UI.ViewModel;
using ModSink.WPF.ViewModel;
using Xunit;

namespace ModSink.WPF.Tests.ViewModel
{
    public class LibraryViewModelTests
    {
        [Fact]
        public void ModpacksContructorInitialization()
        {
            var source = new SourceCache<Modpack, Guid>(_ => Guid.NewGuid());
            var vm = new LibraryViewModel(source);
            vm.Modpacks.Should().HaveCount(0);
            source.AddOrUpdate(new Modpack());
            vm.Modpacks.Should().HaveCount(1);
        }

        [Fact]
        public void SelectedModpackNotifies()
        {
            var source = new SourceCache<Modpack, Guid>(_ => Guid.NewGuid());
            var vm = new LibraryViewModel(source);
            using (var monitor = vm.Monitor())
            {
                vm.SelectedModpack = new Modpack();
                monitor.Should().RaisePropertyChangeFor(x => x.SelectedModpack);
            }
        }
    }
}