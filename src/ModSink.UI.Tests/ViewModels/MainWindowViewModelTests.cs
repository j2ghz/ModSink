using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using ModSink.UI.AvaloniaUI.ViewModels;
using Xunit;

namespace ModSink.UI.Tests.ViewModels
{
    public class MainWindowViewModelTests
    {
        [Fact]
        public void HasGreeting()
        {
            new MainWindowViewModel().Greeting.Should().NotBeNullOrEmpty();
        }
    }
}
