using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using DynamicData;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using ReactiveUI;
using Xunit;

namespace ModSink.Common.Tests
{
    public class ExperimentsTests:ReactiveObject
    {
        private ObservableAsPropertyHelper<IChangeSet<string>> _testProperty;

        public IChangeSet<string> TestProperty { get => _testProperty.Value; }
        private class CustomTestException : Exception
        {
        }

        [Fact]
        public void AsObservableListThrows()
        {
            Assert.Throws<CustomTestException>(() =>
            {
                var executes = false;
                var source = new SourceList<string>();
                var obsList = source.Connect()
                    .ObserveOn(ImmediateScheduler.Instance)
                    .SubscribeOn(ImmediateScheduler.Instance)
                    .Transform(new Func<string, string>(_ =>
                    {
                        executes = true;
                        throw new CustomTestException();
                    }))
                    .AsObservableList();
                //obsList.Connect().Subscribe();
                source.Add(string.Empty);
                Assert.True(executes);
            });
        }

        [Fact]
        public void SourceListThrows()
        {
            var s = new TestScheduler();
            Assert.Throws<CustomTestException>(() =>
            {
                var source = new SourceList<string>();
                var obsChangeSet = source.Connect()
                    .ObserveOn(ImmediateScheduler.Instance)
                    .SubscribeOn(ImmediateScheduler.Instance)
                    .Transform(new Func<string, string>(_ => throw new CustomTestException()));
                _testProperty= obsChangeSet.ToProperty(this, x => x.TestProperty,scheduler:s);
                source.Add(string.Empty);
                s.Start();
            });
        }
    }
}