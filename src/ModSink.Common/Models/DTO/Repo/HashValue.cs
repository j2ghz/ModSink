using System;
using System.Collections.Generic;
using System.Linq;

namespace ModSink.Common.Models.DTO.Repo
{
    [Serializable]
    public struct HashValue : IEquatable<HashValue>, IComparable<HashValue>
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
            return Value?.GetHashCode() ?? 0;
        }

        /// <summary>
        ///     Converts to filename or display string
        /// </summary>
        /// <returns>XX-XX-XX-XX-XX-XX-XX-XX format</returns>
        public override string ToString()
        {
            return BitConverter.ToString(Value);
        }

        /// <inheritdoc />
        public int CompareTo(HashValue other)
        {
            for (var i = 0; i < Math.Max(Value.Length, other.Value.Length); i++)
            {
                var comparison = Value[i].CompareTo(other.Value[i]);
                if (comparison != 0) return comparison;
            }

            return 0;
        }
    }
}