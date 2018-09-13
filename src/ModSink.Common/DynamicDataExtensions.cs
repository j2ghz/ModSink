using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;

namespace ModSink.Common
{
    public static class DynamicDataExtensions
    {
        public static IObservable<IChangeSet<T>> LogVerbose<T>(this IObservable<IChangeSet<T>> source, string prefix)
        {
            return source.Do(changeSet =>
                {
                    foreach (var change in changeSet) LogTo.Verbose("[{prefix}] {change}", prefix, change);
                },
                ex => { LogTo.Warning(ex, "[{prefix}]", prefix); },
                () => { LogTo.Verbose("[{prefix}] Finished", prefix); }
            );
        }
        public static IObservable<IChangeSet<TObject, Tkey>> LogVerbose<TObject,Tkey>(this IObservable<IChangeSet<TObject, Tkey>> source, string prefix)
        {
            return source.Do(changeSet =>
                {
                    foreach (var change in changeSet) LogTo.Verbose("[{prefix}] {change}", prefix, change);
                },
                ex => { LogTo.Warning(ex, "[{prefix}]", prefix); },
                () => { LogTo.Verbose("[{prefix}] Finished", prefix); }
            );
        }
    }
}