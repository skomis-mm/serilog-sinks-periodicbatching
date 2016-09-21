using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching.PerformanceTests.Support;
using Serilog.Tests.Support;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests
{
    public class BoundedQueue_Enqueue_Dequeue_Benchmark
    {
        [Params(100, 1000)]
        public int ItemsCount { get; set; }

        [Params(100, 1000)]
        public int QueueLimit { get; set; }

        readonly LogEvent _logEvent = Some.LogEvent();

        ConcurrentQueue<LogEvent> _concurrentQueue;
        BlockingCollection<LogEvent> _blockingCollection;
        BoundedConcurrentQueue<LogEvent> _boundedConcurrentQueue;
        SynchronizedQueue<LogEvent> _syncronizedQueue;

        [Setup]
        public void Setup()
        {
            _concurrentQueue = new ConcurrentQueue<LogEvent>();
            _blockingCollection = new BlockingCollection<LogEvent>(QueueLimit);
            _boundedConcurrentQueue = new BoundedConcurrentQueue<LogEvent>(QueueLimit);
            _syncronizedQueue = new SynchronizedQueue<LogEvent>(QueueLimit);
        }

        [Benchmark(Baseline = true)]
        public void ConcurrentQueue()
        {
            EnqueueDequeueItems(evt => _concurrentQueue.Enqueue(evt), evt => _concurrentQueue.TryDequeue(out evt));
        }

        [Benchmark]
        public void BoundedConcurrentQueue()
        {
            EnqueueDequeueItems(evt => _boundedConcurrentQueue.TryEnqueue(evt), evt => _boundedConcurrentQueue.TryDequeue(out evt));
        }

        [Benchmark]
        public void BlockingCollection()
        {
            EnqueueDequeueItems(evt => _blockingCollection.TryAdd(evt), evt => _blockingCollection.TryTake(out evt));
        }

        [Benchmark]
        public void SynchronizedQueue()
        {
            EnqueueDequeueItems(evt => _syncronizedQueue.TryEnqueue(evt), evt => _syncronizedQueue.TryDequeue(out evt));
        }

        void EnqueueDequeueItems(Action<LogEvent> enqueueAction, Action<LogEvent> dequeueAction)
        {
            Parallel.Invoke(
                () => Parallel.For(0, ItemsCount, _ => enqueueAction(_logEvent)),
                () => { for (var i = 0; i < ItemsCount; i++) dequeueAction(_logEvent); });
        }
    }
}
