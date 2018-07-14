using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
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
            return o;
        }

        public static IObservableList<T> DisposeWithThrowExceptions<T>(this IObservableList<T> o,
            CompositeDisposable disposable)
        {
            o.DisposeWith(disposable);
            o.Connect().Subscribe().DisposeWith(disposable);
            o.Connect().Subscribe(_ => { },
                ex => RxApp.MainThreadScheduler.Schedule(() => RxApp.DefaultExceptionHandler.OnError(ex)));
            return o;
        }
    }
}