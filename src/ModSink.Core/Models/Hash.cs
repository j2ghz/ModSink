using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace ModSink.Core.Models
{
    [DebuggerDisplay("{value}")]
    public struct Hash : IEquatable<Hash>
    {
        private readonly byte[] value;

        public bool Equals(Hash other) => this.value.Equals(other.value);

        public Hash(byte[] value) => this.value = value;

        public Hash(IEnumerable<byte> value) => this.value = value.ToArray();

        public override int GetHashCode() => this.value.GetHashCode();

        public override string ToString() => Convert.ToBase64String(this.value);
    }
}