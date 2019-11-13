using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using FSharpx;
using Microsoft.FSharp.Core;
using ModSink.Application.Serialization;
using ModSink.Domain.Entities.File;
using PathLib;

namespace ModSink.Application.Tests.Serialization
{
    public abstract class IFormatterTests
    {
        protected abstract IFormatter formatter { get; }


/*        [Property]
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
        }*/

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripRepo(Domain.Entities.Repo.Repo o)
        {
            var stream = formatter.SerializeRepo(o);
            formatter.DeserializeRepo(stream).Should().BeEquivalentTo(o);
        }

//        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
//        public void RoundTripFS(FileSignature o)
//        {
//            var stream = formatter.Serialize(o);
//            formatter.Deserialize<FileSignature>(stream).Should().BeEquivalentTo(o);
//        }

//        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
//        public void RoundTripChunkSignature(ChunkSignature o)
//        {
//            var stream = formatter.Serialize(o);
//            formatter.Deserialize<ChunkSignature>(stream).Should().BeEquivalentTo(o);
//        }

//        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
//        public void RoundTripHash(Hash o)
//        {
//            var stream = formatter.Serialize(o);
//            formatter.Deserialize<ChunkSignature>(stream).Should().BeEquivalentTo(o);
//        }

        public static class RepoGenerators
        {
            public static Arbitrary<Hash> Hash()
            {
                var id = Arb.Default.String().Filter(s => !string.IsNullOrEmpty(s));
                var value = Arb.Default.Array<byte>().Filter(a => a != null && a.Length > 0);

                return Gen.Zip(id.Generator, value.Generator)
                    .ToArbitrary(FSharpFunc.FromFunc<Tuple<string, byte[]>, IEnumerable<Tuple<string, byte[]>>>(t =>
                        id.Shrinker(t.Item1).Zip(value.Shrinker(t.Item2))
                            .Select(tt => new Tuple<string, byte[]>(tt.First, tt.Second)))).Convert(
                        t => new Hash(t.Item1, t.Item2), h => new Tuple<string, byte[]>(h.HashId, h.Value));
            }

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