using System;
using System.Collections.Generic;
using System.Linq;

namespace ModSink.Core.Models.Repo
{
    [Serializable]
    public struct HashValue : IEquatable<HashValue>
    {
        public HashValue(byte[] value)
        {
            Value = value;
        }

        public HashValue(IEnumerable<byte> value)
        {
            Value = value.ToArray();
        }

        public byte[] Value { get; }

        public bool Equals(HashValue other)
        {
            return Value.SequenceEqual(other.Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return BitConverter.ToString(Value);
        }
    }
}