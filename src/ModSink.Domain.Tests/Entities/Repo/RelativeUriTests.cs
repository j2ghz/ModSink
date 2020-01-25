using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
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

    [Theory]
    [InlineData("file:///C:/repo/mod1/", "file:///C:/repo/mod1/a.txt", "a.txt")]
    public void FromAbsolute(string root, string target, string result)
    {
        var uri = RelativeUri.FromAbsolute(new Uri(root), new Uri(target));
        uri.IsAbsoluteUri.Should().BeFalse();
        uri.Should().Be(new RelativeUri(result));
    }
    /// <summary>
    /// HACK: Checks that the hack is still necessary
    /// </summary>
    [Fact]
    public void HackWhenCreatingFromDirectoryInfo()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            {@"/d/a.txt", new MockFileData("a")}
        });
        var d = fileSystem.DirectoryInfo.FromDirectoryName("d");
        d.FullName.Should().NotEndWith("/", "because paths to directories should end with /, but for some reason they don't");

        var wrongUri = RelativeUri.FromAbsolute(new Uri(d.FullName), new Uri(d.GetFiles().Single().FullName));
        wrongUri.ToString().Should().Be("d/a.txt");

        var correctUri = RelativeUri.FromAbsolute(new Uri(d.FullName + "/"), new Uri(d.GetFiles().Single().FullName));
        correctUri.ToString().Should().Be("a.txt");
    }

    [Fact]
    public void EqualsSame()
    {
        new RelativeUri("mod1/a.txt").Should().Be(new RelativeUri("mod1/a.txt"));
    }
}
}