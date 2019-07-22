﻿using FluentAssertions;
using ModSink.Domain.Entities.File;
using Xunit;

namespace ModSink.Domain.Tests.Entities.File
{
    public class FileSignatureTests
    {
        [Fact]
        public void SameEquals()
        {
            var fs1 =  new FileSignature(
                    new TestHash(new byte[]
                    {
                        0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A, 0x23, 0xDC,
                        0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80, 0x77, 0x85, 0xAF, 0xEE,
                        0x48, 0xBB
                    }),
                    1UL);

            var fs2 = new FileSignature(
                new TestHash(new byte[]
                {
                    0xCA, 0x97, 0x81, 0x12, 0xCA, 0x1B, 0xBD, 0xCA, 0xFA, 0xC2, 0x31, 0xB3, 0x9A, 0x23, 0xDC,
                    0x4D, 0xA7, 0x86, 0xEF, 0xF8, 0x14, 0x7C, 0x4E, 0x72, 0xB9, 0x80, 0x77, 0x85, 0xAF, 0xEE,
                    0x48, 0xBB
                }),
                1UL);
            

            fs1.Should().Be(fs2);
        }

        class TestHash : Hash
        {
            public TestHash(byte[] value) : base(value)
            {
            }

            public override string HashId { get; } = "TestHash";
        }
    }
}