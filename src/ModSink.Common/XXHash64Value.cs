using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ModSink.Core.Models;

namespace ModSink.Common
{
    [Serializable]
    [DebuggerDisplay("{Value}")]
    public struct XXHash64Value : IHashValue, IEquatable<XXHash64Value>
    {
        public XXHash64Value(byte[] value) => this.Value = value;

        public XXHash64Value(IEnumerable<byte> value) => this.Value = value.ToArray();

        public byte[] Value { get; }

        public bool Equals(XXHash64Value other) => this.Value.Equals(other.Value);

        public bool Equals(IHashValue other) => (other is XXHash64Value) && (Equals((XXHash64Value)other));

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => Convert.ToBase64String(this.Value);
    }
}