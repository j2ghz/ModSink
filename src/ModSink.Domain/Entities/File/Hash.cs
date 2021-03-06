﻿using System;
using System.Linq;

namespace ModSink.Domain.Entities.File
{
    public class Hash
    {
        public Hash(string hashId, byte[] value)
        {
            //Guard.Argument(hashId, nameof(hashId)).NotNull().NotEmpty();
            //Guard.Argument(value, nameof(value)).NotNull().NotEmpty();
            HashId = hashId;
            Value = value;
        }

        public string HashId { get; }

        public byte[] Value { get; }

        public byte[] RawForHashing()
        {
            var result = HashId.Aggregate(Array.Empty<byte>().AsEnumerable(), (current, t) => current.Append((byte)t));
            result = Value.Aggregate(result, (current, b) => current.Append(b));
            return result.ToArray();
        }

        public override string ToString() => $"Hash(Id='{HashId}',Value='{string.Join(",", Value)}')";
    }
}
