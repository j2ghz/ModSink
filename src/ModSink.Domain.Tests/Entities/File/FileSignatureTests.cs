using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using ModSink.Domain.Entities.File;
using Xunit;

namespace ModSink.Domain.Tests.Entities.File
{
    public class FileSignatureTests
    {
        [Property]
        public Property SameEqualsProperty(string id, byte[] value, long length)
        {
            return new Func<bool>(() => new Signature(new Hash(id, value), length) ==
                    new Signature(new Hash($"{id}", value), length))
                .When(!string.IsNullOrEmpty(id) && !(value is null) && value.Length > 0);
        }

        [Fact]
        public void SameEquals()
        {
            var fs1 = new Signature(
                new Hash("test", new byte[]
                {
                    0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A, 0x23, 0xDC,
                    0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80, 0x77, 0x85, 0xAF, 0xEE,
                    0x48, 0xBB
                }),
                1L);

            var fs2 = new Signature(
                new Hash("test", new byte[]
                {
                    0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A, 0x23, 0xDC,
                    0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80, 0x77, 0x85, 0xAF, 0xEE,
                    0x48, 0xBB
                }),
                1L);


            fs1.Should().Be(fs2);
        }
    }
}
