using System;
using System.Diagnostics;
using System.IO;

namespace ModSink.Domain.Entities.File
{
    public struct FileSignature : IEquatable<FileSignature>
    {
        public FileSignature(Hash hash, ulong length)
        {
            Hash = hash;
            Length = length;
        }

        public FileSignature(Hash hash, long length)
        {
            Hash = hash;
            Length = Convert.ToUInt64(length);
        }

        public Hash Hash { get; }

        /// <summary>
        ///     The length of the file taken from <see cref="FileInfo" />, in bytes.
        /// </summary>
        public ulong Length { get; }

        #region Generated

        public bool Equals(FileSignature other)
        {
            return Equals(Hash, other.Hash) && Length == other.Length;
        }

        public override bool Equals(object obj)
        {
            return obj is FileSignature other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hash != null ? Hash.GetHashCode() : 0) * 397) ^ Length.GetHashCode();
            }
        }

        public static bool operator ==(FileSignature left, FileSignature right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FileSignature left, FileSignature right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}