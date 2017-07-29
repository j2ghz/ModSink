using ModSink.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ModSink.Common
{
    [Serializable]
    [DebuggerDisplay("{Value}")]
    public struct ByteHashValue : IHashValue, IEquatable<ByteHashValue>
    {
        public ByteHashValue(byte[] value) => this.Value = value;

        public ByteHashValue(IEnumerable<byte> value) => this.Value = value.ToArray();

        public ByteHashValue(string base64string) => this.Value = Convert.FromBase64String(base64string);

        public byte[] Value { get; }

        public bool Equals(ByteHashValue other) => this.Value.Equals(other.Value);

        public bool Equals(IHashValue other) => (other is ByteHashValue) && (Equals((ByteHashValue)other));

        public override int GetHashCode() => this.Value.GetHashCode();

        public override string ToString() => BitConverter.ToString(this.Value);
    }
}