using System;
using FluentAssertions;
using ModSink.Domain.Entities.Repo;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo
{
    public class RelativeUriTests
    {
        [Theory]
        [InlineData("https://example.com/")]
        public void ConstructFailsFromInvalidString(string url)
        {
            try
            {
                var uri = new RelativeUri(url);
                uri.ToString().Should().Be("", "because AbosluteUris shouldn't make relative ones");
            }
            catch (UriFormatException)
            {
                return;
            }

            throw new NotImplementedException();
        }

        [Theory]
        [InlineData(".")]
        [InlineData("..")]
        [InlineData("a")]
        [InlineData("a/b")]
        [InlineData("a/b.txt")]
        public void ConstructsFromValidString(string url)
        {
            var uri = new RelativeUri(url);
            uri.IsAbsoluteUri.Should().BeFalse();
        }
    }
}