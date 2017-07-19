using System;

namespace ModSink.Core.Models
{
    /// <summary>
    /// Represents a hash
    /// </summary>
    public interface IHashValue : IEquatable<IHashValue>
    {
        /// <summary>
        /// String representation of the hash.
        /// </summary>
        /// <returns>String representation of the hash. Usually <see cref="Convert.ToBase64String(byte[])"/></returns>
        /// <remarks>Can be used as a filename, so make sure its length is reasonable. It is recommended to use <see cref="Convert.ToBase64String(byte[])"/></remarks>
        string ToString();
    }
}