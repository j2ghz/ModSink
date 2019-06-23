using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Infrastructure.Hashing;
using Xunit;

namespace ModSink.Infrastructure.Tests.Hashing
{
    public class FileStreamOpenerTests
    {
        [Fact]
        public void OpenRead()
        {
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {@"c:\myfile.txt", new MockFileData("Testing is meh.")}
            });
            var fsf = new MockFileStreamFactory(fileSystem);
            var opener = new FileStreamOpener(Options.Create(new FileStreamOpener.Options()), fsf);
            var stream = opener.OpenRead(fileSystem.FileInfo.FromFileName(@"c:\myfile.txt"));
            using var reader = new StreamReader(stream);
            reader.ReadToEnd().Should().Be("Testing is meh.");
        }
    }
}