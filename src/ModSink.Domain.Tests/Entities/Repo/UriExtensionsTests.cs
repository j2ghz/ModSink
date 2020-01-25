using FluentAssertions;
using ModSink.Domain.Entities.Repo;
using System;
using System.Collections.Generic;
using System.Text;
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
            UriExtensions.ToSerializableString(uri).Should().BeEquivalentTo(input);
        }
    }
}
