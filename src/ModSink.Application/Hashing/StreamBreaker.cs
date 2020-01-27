using System;
using System.Collections.Generic;
using System.IO;
using ModSink.Domain.Entities.File;

namespace ModSink.Application.Hashing
{
public class StreamBreaker
{
    private const int bufferSize = 64 * 1024;
    private const long seed = 2273; //--> a our hash seed
    private const int width = 64; //--> the # of bytes in the window
    private static object sync = new object();
    private readonly IHashFunction _hashFunction;
    private readonly long mask; //--> a hash seive: 16 gets you ~64k chunks


    /// <param name="sieveSize">16 gives ~64k chunks</param>
    public StreamBreaker(IHashFunction hashFunction, byte sieveSize = 9)
    {
        _hashFunction = hashFunction;
        mask = (1 << sieveSize) - 1;
    }

    /// <summary>
    ///     Subdivides a stream using a Rabin-Karp rolling hash to find sentinal locations
    /// </summary>
    /// <param name="stream">The stream to read</param>
    /// <param name="length">The length to read</param>
    /// <param name="hasher">A hash algorithm to create a strong hash over the segment</param>
    /// <remarks>
    ///     We may be reading a stream out of a BackupRead operation.
    ///     In this case, the stream <b>might not</b> be positioned at 0
    ///     and may be longer than the section we want to read.
    ///     So...we keep track of length and don't arbitrarily read 'til we get zero
    ///     Also, overflows occur when getting maxSeed and when calculating the hash.
    ///     These overflows are expected and not significant to the computation.
    /// </remarks>
    public IEnumerable<Segment> GetSegments(Stream stream, long length)
    {
        var maxSeed = seed; //--> will be prime^width after initialization (sorta)
        var buffer = new byte[bufferSize];
        var circle = new byte[width]; //--> the circular queue: input to the hash functions
        var hash = 0L; //--> our rolling hash
        var circleIndex = 0; //--> index into circular queue
        var last = 0L; //--> last place we started a new segment
        var pos = 0L; //--> the position we're at in the range of stream we're reading
        var hasher = _hashFunction.AsHashAlgorithm();

        //--> initialize maxSeed...
        for (var i = 0; i < width; i++) maxSeed *= maxSeed;

        while (true)
        {
            //--> Get some bytes to work on (don't let it read past length)
            var bytesRead = stream.Read(buffer, 0, (int)Math.Min(bufferSize, length - pos));
            for (var i = 0; i < bytesRead; i++)
            {
                pos++;
                hash = buffer[i] + (hash - maxSeed * circle[circleIndex]) * seed;
                circle[circleIndex++] = buffer[i];
                if (circleIndex == width) circleIndex = 0;
                if ((hash | mask) == hash || pos == length) //--> match or EOF
                {
                    //--> apply the strong hash to the remainder of the bytes in the circular queue...
                    hasher.TransformFinalBlock(circle, 0, circleIndex == 0 ? width : circleIndex);

                    //--> return the results to the caller...
                    yield return new Segment(last, pos - last, _hashFunction.CreateHash(hasher.Hash));
                    last = pos;

                    //--> reset the hashes...
                    hash = 0;
                    for (var j = 0; j < width; j++) circle[j] = 0;

                    circleIndex = 0;
                    hasher.Initialize();
                }
                else
                {
                    if (circleIndex == 0) hasher.TransformBlock(circle, 0, width, circle, 0);
                }
            }

            if (bytesRead == 0) break;
        }
    }

    /// <summary>A description of a chunk</summary>
    public struct Segment
    {
        /// <summary>How far into the strem we found the chunk</summary>
        public readonly long Offset;

        /// <summary>How long the chunk is in bytes</summary>
        public readonly long Length;

        /// <summary>Strong hash for the chunk</summary>
        public readonly Hash Hash;

        internal Segment(long offset, long length, Hash hash)
        {
            Offset = offset;
            Length = length;
            Hash = hash;
        }

        public Chunk ToChunk()
        {
            return new Chunk { Position = Offset, Signature = new Signature(Hash, Length) }
                   ;
        }
    }
}
}
