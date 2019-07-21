using System;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using ModSink.Application.Serialization;
using Xunit;

namespace ModSink.Application.Tests.Serialization
{
    public abstract class IFormatterTests
    {
        protected abstract IFormatter formatter { get; }


        [Property]
        public void CanDeserialize(string ext)
        {
            try
            {
                formatter.CanDeserialize(ext);
            }
            catch (ArgumentNullException) when (ext==null)
            {
               //Exception expected
            }
            
        }

        [Property]
        public void RoundTripString(string o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<string>(stream).Should().BeEquivalentTo(o);
        }

        [Property]
        public void RoundTripRepo(Domain.Entities.Repo.Repo o)
        {
            var stream = formatter.Serialize(o);
            formatter.Deserialize<Domain.Entities.Repo.Repo>(stream).Should().BeEquivalentTo(o);
        }
    }
}