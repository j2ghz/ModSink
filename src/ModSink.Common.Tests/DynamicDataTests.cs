using System.Collections.Generic;
using System.Linq;
using DynamicData;
using FluentAssertions;
using Xunit;

namespace ModSink.Common.Tests
{
    public class DynamicDataTests
    {
        [Fact]
        public void TopShouldRefresh()
        {
            var source = new SourceCache<int, int>(i => i);
            var dest = source.Connect().Top(Comparer<int>.Create((_, __) => 0), 2).AsObservableCache();
            source.AddOrUpdate(Enumerable.Range(0, 5));
            dest.Items.Should().BeEquivalentTo(Enumerable.Range(0, 2));
            source.RemoveKey(0);
            source.RemoveKey(1);
            dest.Items.Should().BeEquivalentTo(Enumerable.Range(2, 2));
        }
    }
}