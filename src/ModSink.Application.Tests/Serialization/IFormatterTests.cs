using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using FSharpx;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Tests.Serialization
{
    public abstract class IFormatterTests
    {
        protected abstract IFormatter formatter { get; }


        [Property]
        public void CanDeserialize(string ext)
        {
            try
            {
                formatter.CanDeserialize(ext);
            }
            catch (ArgumentNullException) when (ext == null)
            {
                //Exception expected
            }
        }

        [Property]
        public void RoundTripString(string o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<string>(stream).Should().BeEquivalentTo(o);
        }

        [Property(Arbitrary = new[] {typeof(RepoGenerators)}, Skip = "Unfinished")]
        public void RoundTripRepo(Domain.Entities.Repo.Repo o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<Domain.Entities.Repo.Repo>(stream).Should().BeEquivalentTo(o);
        }

        public static class RepoGenerators
        {
            public static Arbitrary<IReadOnlyCollection<T>> IReadOnlyCollection<T>()
            {
                return Arb.Convert(FSharpFunc.FromFunc((ICollection<T> c) => (IReadOnlyCollection<T>) c),
                    FSharpFunc.FromFunc((IReadOnlyCollection<T> c) => (ICollection<T>) c.ToList()),
                    Arb.Default.ICollection<T>());
            }

            public static Gen<FileSignature> FileSignature()
            {
                return from hashId in Arb.Default.String().Generator
                    from value in Arb.Default.Array<byte>().Generator
                    from length in Arb.Default.Int64().Generator
                    select new FileSignature(new TestHash(hashId, value), length);
            }

            public class TestHash : Hash
            {
                public TestHash(string id, byte[] value) : base(value)
                {
                    HashId = id;
                }

                public override string HashId { get; }
            }
        }
    }
}