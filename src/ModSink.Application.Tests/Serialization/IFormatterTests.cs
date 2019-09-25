using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using FSharpx;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using PathLib;

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

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripRepo(Domain.Entities.Repo.Repo o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<Domain.Entities.Repo.Repo>(stream).Should().BeEquivalentTo(o);
        }

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripFS(FileSignature o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<FileSignature>(stream).Should().BeEquivalentTo(o);
        }

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripChunkSignature(ChunkSignature o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<ChunkSignature>(stream).Should().BeEquivalentTo(o);
        }

        public static class RepoGenerators
        {
            public static Arbitrary<IReadOnlyCollection<T>> IReadOnlyCollection<T>()
            {
                return Arb.Convert(FSharpFunc.FromFunc((ICollection<T> c) => (IReadOnlyCollection<T>) c),
                    FSharpFunc.FromFunc((IReadOnlyCollection<T> c) => (ICollection<T>) c.ToList()),
                    Arb.Default.ICollection<T>());
            }

            public static Arbitrary<IReadOnlyDictionary<TKey, TValue>> IReadOnlyDictionary<TKey, TValue>()
            {
                return Arb.Convert(
                    FSharpFunc.FromFunc((IDictionary<TKey, TValue> c) => (IReadOnlyDictionary<TKey, TValue>) c),
                    FSharpFunc.FromFunc((IReadOnlyDictionary<TKey, TValue> c) =>
                        (IDictionary<TKey, TValue>) c.ToList()),
                    Arb.Default.IDictionary<TKey, TValue>());
            }

            public static Arbitrary<IPurePath> IPurePath()
            {
                return Arb.Filter(FSharpFunc.FromFunc<string, bool>(p => new PurePathFactory().TryCreate(p, out _)),
                        Arb.Default.String())
                    .Convert(s => new PurePathFactory().Create(s), path => path.ToString());
            }


        }
    }
}