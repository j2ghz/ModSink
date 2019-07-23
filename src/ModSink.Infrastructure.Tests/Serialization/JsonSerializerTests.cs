using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using ModSink.Application.Hashing;
using ModSink.Application.Serialization;
using ModSink.Application.Tests.Serialization;
using ModSink.Infrastructure.Hashing;
using ModSink.Infrastructure.Serialization;
using Moq;
using Xunit;

namespace ModSink.Infrastructure.Tests.Serialization
{
    public class JsonSerializerTests : IFormatterTests
    {
        protected override IFormatter formatter => new JsonSerializer();
    }
}