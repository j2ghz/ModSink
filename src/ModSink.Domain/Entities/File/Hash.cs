﻿using System;

namespace ModSink.Domain.Entities.File
{
    public class Hash :  IEquatable<Hash>
    {
        public Hash(byte[] value, string hashId)
        {
            Value = value;
            HashId = hashId;
        }

        public string HashId { get; }

        public byte[] Value { get; }

        #region Generated

        public bool Equals(Hash other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(HashId, other.HashId) && Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Hash)obj);
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

    }
}