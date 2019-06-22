using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using ModSink.Domain.Entities.Repo;
using Xunit;

namespace ModSink.Domain.Tests.Entities.Repo
{
    public class RelativeUriTests
    {
        public static IEnumerable<object[]> ValidUris = new List<string>
        {
            ".", "..", "a", "a/b", "a/b.txt"
        }.Select(s => new object[] {s});

        public static IEnumerable<object[]> InvalidUris = new List<string>
        {
            "https://example.com/"
        }.Select(s => new object[] {s});

        [Theory]
        [MemberData(nameof(ValidUris))]
        public void ConstructsFromValidString(string url)
        {
            var uri = new RelativeUri(url);
            uri.IsAbsoluteUri.Should().BeFalse();
        }

        [Theory]
        [MemberData(nameof(InvalidUris))]
        public void ConstructFailsFromInvalidString(string url)
        {
            try
            {
                var uri = new RelativeUri(url);
                uri.ToString().Should().Be("", "because AbosluteUris shouldn't make relative ones");
            }
            catch (UriFormatException ex)
            {
                return;
            }

            throw new NotImplementedException();
        }
    }
}