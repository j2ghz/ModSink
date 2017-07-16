using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace ModSink.Core.Models
{
    [DebuggerDisplay("{value}")]
    public struct Hash : IEquatable<Hash>, IFormattable
    {
        private readonly byte[] value;

        public bool Equals(Hash other)
        {
            return this.value.Equals(other.value);
        }

        public Hash(byte[] value)
        {
            this.value = value;
        }

        public Hash(IEnumerable<byte> value)
        {
            this.value = value.ToArray();
        }

        public override int GetHashCode()
        {
            return this.value.GetHashCode();
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return Convert.ToBase64String(this.value);
        }
    }
}
