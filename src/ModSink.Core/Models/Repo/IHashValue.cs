using System;

namespace ModSink.Core.Models.Repo
{
    /// <summary>
    /// Represents a hash
    /// </summary>
    public interface IHashValue : IEquatable<IHashValue>
    {
        /// <summary>
        /// String representation of the hash.
        /// </summary>
        /// <returns>String representation of the hash. Usually <see cref="BitConverter(byte[])"/></returns>
        /// <remarks>Can be used as a filename, so make sure its length is reasonable and is case-insensitive. It is recommended to use <see cref="Convert.ToBase64String(byte[])"/></remarks>
        string ToString();
    }
}