using System;
using FluentAssertions;
using ModSink.Common.Models;
using Xunit;

namespace ModSink.Common.Tests.Models
{
    public class WithBaseUriTests
    {
        [Theory]
        [InlineData("http://example.com/group.bin", "Repo/repo.bin", "http://example.com/Repo/repo.bin")]
        [InlineData("http://example.com/Repo/repo.bin","Modpack/Mod/File.bin", "http://example.com/Repo/Modpack/Mod/File.bin")]
        public void CombinesCorrectly(string @base, string relative, string expected)
        {
            var withBaseUri = new WithBaseUriTestClass(){BaseUri = new Uri(@base) };
            var actual = withBaseUri.CombineBaseUri(new Uri(relative,UriKind.Relative));
            actual.Should().Be(new Uri(expected));
        }
        private class WithBaseUriTestClass : WithBaseUri
        {

        }
    }
}