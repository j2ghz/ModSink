using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ModSink.Core.Models;

namespace ModSink.Common
{
    [Serializable]
    [DebuggerDisplay("{Value}")]
    public struct XXHash64 : IHashValue, IEquatable<XXHash64>
    {
        public XXHash64(byte[] value) => this.Value = value;

        public XXHash64(IEnumerable<byte> value) => this.Value = value.ToArray();

        public byte[] Value { get; }

        public bool Equals(XXHash64 other) => this.Value.Equals(other.Value);

        public bool Equals(IHash other) => (other is XXHash64) && (Equals((XXHash64)other));

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => Convert.ToBase64String(this.Value);
    }
}