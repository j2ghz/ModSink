using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Anotar.Serilog;
using DynamicData;
using ReactiveUI;

namespace ModSink.Common
{
    public static class DynamicDataExtensions
    {
        public static IObservableCache<TObject, TKey> DisposeWithThrowExceptions<TObject, TKey>(
            this IObservableCache<TObject, TKey> o,
            CompositeDisposable disposable)
        {
            o.DisposeWith(disposable);
            o.Connect().Subscribe().DisposeWith(disposable);
            o.Connect().Subscribe(_ => { },
                ex => RxApp.MainThreadScheduler.Schedule(() => RxApp.DefaultExceptionHandler.OnError(ex)));
            o.Connect().Subscribe(_ => { }, _ => Debugger.Break());
            return o;
        }

        public static IObservableList<T> DisposeWithThrowExceptions<T>(this IObservableList<T> o,
            CompositeDisposable disposable)
        {
            o.DisposeWith(disposable);
            o.Connect().Subscribe().DisposeWith(disposable);
            o.Connect().Subscribe(_ => { },
                ex => RxApp.MainThreadScheduler.Schedule(() => RxApp.DefaultExceptionHandler.OnError(ex)));
            o.Connect().Subscribe(_ => { }, _ => Debugger.Break());
            return o;
        }

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