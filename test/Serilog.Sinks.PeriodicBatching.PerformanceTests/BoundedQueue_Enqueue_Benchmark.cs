using BenchmarkDotNet.Attributes;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching.PerformanceTests.Support;
using Serilog.Tests.Support;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests
{
    public class BoundedQueue_Enqueue_Benchmark
    {
        [Params(100, 1000)]
        public int ItemsCount { get; set; }

        [Params(100, 1000)]
        public int QueueLimit { get; set; }

        [Params(1, -1)]
        public int MaxDegreeOfParallelism { get; set; }

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
            EnqueueItems(evt => _concurrentQueue.Enqueue(evt));
        }

        [Benchmark]
        public void BoundedConcurrentQueue()
        {
            EnqueueItems(evt => _boundedConcurrentQueue.TryEnqueue(evt));
        }

        [Benchmark]
        public void BlockingCollection()
        {
            EnqueueItems(evt => _blockingCollection.TryAdd(evt));
        }

        [Benchmark]
        public void SynchronizedQueue()
        {
            EnqueueItems(evt => _syncronizedQueue.TryEnqueue(evt));
        }

        void EnqueueItems(Action<LogEvent> enqueueAction)
        {
            Parallel.For(0, ItemsCount, new ParallelOptions() { MaxDegreeOfParallelism = MaxDegreeOfParallelism }, _ => enqueueAction(_logEvent));
        }
    }
}
