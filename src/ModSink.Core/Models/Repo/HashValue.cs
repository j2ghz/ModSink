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

        /// <summary>
        ///     Reverses <see cref="HashValue" />.<see cref="ToString" />
        /// </summary>
        /// <param name="str">Result of <see cref="HashValue" />.<see cref="ToString" /></param>
        public HashValue(string str)
        {
            Value = Array.ConvertAll(str.Split('-'), s => Convert.ToByte(s, 16));
        }

        public byte[] Value { get; }

        public bool Equals(HashValue other)
        {
            return Value.SequenceEqual(other.Value);
        }

        public override bool Equals(object obj)
        {
            if (obj is HashValue value) return Equals(value);
            return base.Equals(obj);
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