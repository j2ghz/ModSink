using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Anotar.Serilog;
using Humanizer;
using ReactiveUI;

namespace ModSink.WPF.Services
{
    public sealed class Memory : IEquatable<Memory>
    {
        public Memory(decimal workingSetPrivate, decimal managed)
        {
            WorkingSetPrivate = workingSetPrivate;
            Managed = managed;
        }

        public decimal WorkingSetPrivate { get; }

        public decimal Managed { get; }

        public bool Equals(Memory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return WorkingSetPrivate == other.WorkingSetPrivate && Managed == other.Managed;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Memory && Equals((Memory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (WorkingSetPrivate.GetHashCode() * 397) ^ Managed.GetHashCode();
            }
        }

        public static bool operator ==(Memory left, Memory right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Memory left, Memory right)
        {
            return !Equals(left, right);
        }
    }

    public sealed class DiagnosticsService : IDisposable
    {
        private readonly IConnectableObservable<Counters> _countersObservable;
        private readonly CompositeDisposable _disposable;
        private readonly IConnectableObservable<int> _rpsObservable;
        private readonly Queue<long> _rpsQueue;
        private readonly object _sync;
        private bool _countersConnected;

        private bool _rpsConnected;

        public DiagnosticsService(DispatcherObject mainWindow)
        {
            _disposable = new CompositeDisposable();

            _sync = new object();
            var idle = Observable.FromEventPattern(h => mainWindow.Dispatcher.Hooks.DispatcherInactive += h,
                    h => mainWindow.Dispatcher.Hooks.DispatcherInactive -= h, RxApp.TaskpoolScheduler)
                .Buffer((2 / 3D).Seconds(), RxApp.TaskpoolScheduler).Where(x => x.Any());

            _countersObservable = Observable.Create<Counters>(x =>
                {
                    var disposable = new CompositeDisposable();

                    try
                    {
                        var processName = GetProcessInstanceName();

                        LogTo.Information("Creating performance counter 'Working Set'");

                        var workingSetCounter = new PerformanceCounter("Process", "Working Set", processName);
                        disposable.Add(workingSetCounter);

                        LogTo.Information("Creating performance counter '% Processor Time'");

                        var cpuCounter = new PerformanceCounter("Process", "% Processor Time", processName);
                        disposable.Add(cpuCounter);


                        workingSetCounter.NextValue();
                        cpuCounter.NextValue();

                        x.OnNext(new Counters(workingSetCounter, cpuCounter));

                        LogTo.Information("Ready");
                    }
                    catch (ArgumentException exn)
                    {
                        LogFailToCreatePerformanceCounter(x, exn);
                    }
                    catch (InvalidOperationException exn)
                    {
                        LogFailToCreatePerformanceCounter(x, exn);
                    }
                    catch (Win32Exception exn)
                    {
                        LogFailToCreatePerformanceCounter(x, exn);
                    }
                    catch (PlatformNotSupportedException exn)
                    {
                        LogFailToCreatePerformanceCounter(x, exn);
                    }
                    catch (UnauthorizedAccessException exn)
                    {
                        LogFailToCreatePerformanceCounter(x, exn);
                    }

                    return disposable;
                })
                .DelaySubscription(1.Seconds(), RxApp.TaskpoolScheduler)
                .SubscribeOn(RxApp.TaskpoolScheduler)
                .ObserveOn(RxApp.TaskpoolScheduler)
                .CombineLatest(idle, (x, y) => x)
                .Replay(1);

            _rpsQueue = new Queue<long>();
            _rpsObservable = Observable.FromEventPattern<EventHandler, EventArgs>(h => CompositionTarget.Rendering += h,
                    h => CompositionTarget.Rendering -= h)
                .Synchronize()
                .Select(x => CalculateRps())
                .Publish();
        }

        public IObservable<Memory> Memory
        {
            get
            {
                ConnectCountersObservable();

                return _countersObservable.Select(CalculateMemoryValues)
                    .DistinctUntilChanged();
            }
        }

        public IObservable<int> Cpu
        {
            get
            {
                ConnectCountersObservable();

                return _countersObservable.Select(CalculateCpu)
                    .Delay((2 / 3D).Seconds(), RxApp.TaskpoolScheduler)
                    .DistinctUntilChanged()
                    .Select(DivideByNumberOfProcessors);
            }
        }

        public IObservable<int> Rps
        {
            get
            {
                ConnectRpsObservable();

                return _rpsObservable.DistinctUntilChanged();
            }
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private static void LogFailToCreatePerformanceCounter(IObserver<Counters> counters, Exception exception)
        {
            LogTo.Error(exception, "Failed to create performance counters!");
            counters.OnError(exception);
        }

        private static void LogFailToCalculateMemory(Exception exception)
        {
            LogTo.Warning(exception, "Failed to calculate memory!");
        }

        private static void LogFailToCalculateCpu(Exception exception)
        {
            LogTo.Warning(exception, "Failed to calculate cpu!");
        }

        private static Memory CalculateMemoryValues(Counters counters)
        {
            try
            {
                var rawValue = counters.WorkingSet.NextValue();
                var privateWorkingSet = Convert.ToDecimal(rawValue);

                var managed = GC.GetTotalMemory(false);

                return new Memory(privateWorkingSet, managed);
            }
            catch (InvalidOperationException exn)
            {
                LogFailToCalculateMemory(exn);
                return new Memory(0, 0);
            }
            catch (Win32Exception exn)
            {
                LogFailToCalculateMemory(exn);
                return new Memory(0, 0);
            }
            catch (PlatformNotSupportedException exn)
            {
                LogFailToCalculateMemory(exn);
                return new Memory(0, 0);
            }
            catch (UnauthorizedAccessException exn)
            {
                LogFailToCalculateMemory(exn);
                return new Memory(0, 0);
            }
            catch (OverflowException exn)
            {
                LogFailToCalculateMemory(exn);
                return new Memory(0, 0);
            }
        }

        private static int CalculateCpu(Counters counters)
        {
            try
            {
                var rawValue = counters.Cpu.NextValue();
                return Convert.ToInt32(rawValue);
            }
            catch (InvalidOperationException exn)
            {
                LogFailToCalculateCpu(exn);
                return 0;
            }
            catch (Win32Exception exn)
            {
                LogFailToCalculateCpu(exn);
                return 0;
            }
            catch (PlatformNotSupportedException exn)
            {
                LogFailToCalculateCpu(exn);
                return 0;
            }
            catch (UnauthorizedAccessException exn)
            {
                LogFailToCalculateCpu(exn);
                return 0;
            }
            catch (OverflowException exn)
            {
                LogFailToCalculateCpu(exn);
                return 0;
            }
        }

        private static int DivideByNumberOfProcessors(int value)
        {
            try
            {
                return value == 0 ? 0 : value / Environment.ProcessorCount;
            }
            catch (OverflowException)
            {
                return 0;
            }
        }

        private static string GetProcessInstanceName()
        {
            var currentProcess = Process.GetCurrentProcess();
            foreach (var instance in new PerformanceCounterCategory("Process").GetInstanceNames()
                .Where(x => x.StartsWith(currentProcess.ProcessName, StringComparison.InvariantCulture)))
                try
                {
                    using (var counter = new PerformanceCounter("Process", "ID Process", instance, true))
                    {
                        var val = (int) counter.RawValue;
                        if (val == currentProcess.Id) return instance;
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (InvalidOperationException)
                {
                }
                catch (Win32Exception)
                {
                }
                catch (PlatformNotSupportedException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }

            throw new ArgumentException(
                @"Could not find performance counter instance name for current process, name '{0}'",
                currentProcess.ProcessName);
        }

        private void ConnectCountersObservable()
        {
            if (_countersConnected) return;

            lock (_sync)
            {
                if (_countersConnected) return;

                var disposable = _countersObservable.Connect();
                _disposable.Add(disposable);

                _countersConnected = true;
            }
        }

        private void ConnectRpsObservable()
        {
            if (_rpsConnected) return;

            lock (_sync)
            {
                if (_rpsConnected) return;

                var disposable = _rpsObservable.Connect();
                _disposable.Add(Disposable.Create(() =>
                {
                    disposable.Dispose();
                    _rpsQueue.Clear();
                }));

                _rpsConnected = true;
            }
        }

        private int CalculateRps()
        {
            var now = DateTime.Now;
            var endTime = now.Ticks;
            var startTime = now.AddSeconds(-1).Ticks;

            while (_rpsQueue.Any())
            {
                if (_rpsQueue.Peek() < startTime)
                {
                    _rpsQueue.Dequeue();

                    continue;
                }

                break;
            }

            _rpsQueue.Enqueue(endTime);
            return _rpsQueue.Count;
        }

        internal sealed class Counters
        {
            public Counters(PerformanceCounter workingSetCounter, PerformanceCounter cpuCounter)
            {
                WorkingSet = workingSetCounter;
                Cpu = cpuCounter;
            }

            public PerformanceCounter WorkingSet { get; }

            public PerformanceCounter Cpu { get; }
        }
    }
}