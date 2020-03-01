using System;
using System.Collections.Generic;
using System.ComponentModel;
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


        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundtripMapIPurePath(IPurePath p)
        {
            var str = TypeDescriptor.GetConverter(typeof(IPurePath)).ConvertToInvariantString(p);
            var res = (IPurePath)TypeDescriptor.GetConverter(typeof(IPurePath))
                .ConvertFromInvariantString(str);
            res.Should().BeEquivalentTo(str, c => c.WithTracing());
        }

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripRepo(Domain.Entities.Repo.Repo o)
        {
            using var stream = formatter.SerializeRepo(o);
            stream.Position = 0;
            var deserialized = formatter.DeserializeRepo(stream);
            deserialized.Should().BeEquivalentTo(o,
                c => c.WithTracing(),
                "serialization roundtrip should not change the repo");
            //deserialized.Should().Be(o, "equivalence succeeded");
        }

        [Property(Arbitrary = new[] {typeof(RepoGenerators)})]
        public void RoundTripRepoMapOnly(Domain.Entities.Repo.Repo o)
        {
            var mapped = formatter.MapAndBack(o);
            mapped.Should().BeEquivalentTo(o, c => c.WithTracing(),
                "mapping should not change the repo");
            var mappedTwice = formatter.MapAndBack(mapped);
            mappedTwice.Should().BeEquivalentTo(mappedTwice, c => c.WithTracing(),
                "mapping a mapped repo should not change it");
            mappedTwice.Should().BeEquivalentTo(o, c => c.WithTracing(),
                "mapping a mapped repo should not change it");
            //mapped.Should().Be(o, "equivalence succeeded");
        }

        private static class RepoGenerators
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

            public static Arbitrary<IPurePath> IPurePath() =>
                Arb.Filter(FSharpFunc.FromFunc<string, bool>(p => new PurePathFactory().TryCreate(p, out _)),
                        Arb.Default.String())
                    .Convert(s => new PurePathFactory().Create(s), path => path.ToString());

            public static Arbitrary<IReadOnlyCollection<T>> IReadOnlyCollection<T>() =>
                Arb.Convert(FSharpFunc.FromFunc((ICollection<T> c) => (IReadOnlyCollection<T>)c),
                    FSharpFunc.FromFunc((IReadOnlyCollection<T> c) => (ICollection<T>)c.ToList()),
                    Arb.Default.ICollection<T>());

            public static Arbitrary<IReadOnlyDictionary<TKey, TValue>> IReadOnlyDictionary<TKey, TValue>() =>
                Arb.Convert(
                    FSharpFunc.FromFunc((IDictionary<TKey, TValue> c) => (IReadOnlyDictionary<TKey, TValue>)c),
                    FSharpFunc.FromFunc((IReadOnlyDictionary<TKey, TValue> c) =>
                        (IDictionary<TKey, TValue>)c.ToList()),
                    Arb.Default.IDictionary<TKey, TValue>());

            public static Arbitrary<string> String() => Arb.Default.String().Filter(s => s != null);
        }
    }
}
