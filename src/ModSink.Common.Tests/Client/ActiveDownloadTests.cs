using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using ModSink.Common.Client;
using Xunit;

namespace ModSink.Common.Tests.Client
{
    public class ActiveDownloadTests
    {
        [Fact]
        public void FinishActionCalls()
        {
            var finishedCalled = false;
            var prog = new Subject<DownloadProgress>();
            var ad = new ActiveDownload(prog.Publish(), () => finishedCalled = true, "");
            prog.OnCompleted();
            finishedCalled.Should().BeTrue("because ActiveDownload should be called when download is finished");
        }

    }
}