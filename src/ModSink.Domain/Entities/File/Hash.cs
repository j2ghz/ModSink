using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;

namespace ModSink.Domain.Entities.File
{
    public sealed class Hash : IEquatable<Hash>
    {
        public Hash(string id, byte[] value)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            Guard.Argument(value, nameof(value)).NotNull().NotEmpty();
            HashId = id;
            Value = value;
        }
       

        public  string HashId { get;  }

        public byte[] Value { get;  }

        #region Generated

        public bool Equals(Hash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(HashId, other.HashId) && ( (Value==null && other.Value == null) || Value.SequenceEqual(other.Value));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Hash) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((HashId != null ? HashId.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Hash left, Hash right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Hash left, Hash right)
        {
            return !Equals(left, right);
        }

        #endregion

        public byte[] RawForHashing()
        {
            var result = HashId.Aggregate(new byte[0].AsEnumerable(), (current, t) => current.Append((byte) t));
            result = Value.Aggregate(result, (current, b) => current.Append(b));
            return result.ToArray();
        }
    }
}