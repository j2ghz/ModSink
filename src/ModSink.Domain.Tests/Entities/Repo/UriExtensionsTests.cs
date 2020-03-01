using System;
using FluentAssertions;
using ModSink.Domain.Entities.Repo;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo
{
    public class UriExtensionsTests
    {
        [Theory]
        [InlineData("Mod/a.b")]
        public void GetSerializableUriString(string input)
        {
            var uri = new Uri(input, UriKind.Relative);
            uri.ToSerializableString().Should().BeEquivalentTo(input);
            //UriExtensions.ToUri(input).ToSerializableString().Should().BeEquivalentTo(input);
        }
    }
}
