using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Serilog.Sinks.PeriodicBatching
{
    class BoundedConcurrentQueue<T> 
    {
        readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        readonly int _queueLimit;

        int _counter;

        public BoundedConcurrentQueue(): this(int.MaxValue) { }

        public BoundedConcurrentQueue(int queueLimit)
        {
            if (queueLimit <= 0)
                throw new ArgumentOutOfRangeException(nameof(queueLimit), "queue limit must be positive");

            _queueLimit = queueLimit;
        }

        public int Count => _queue.Count;

        public bool TryDequeue(out T item)
        {
            if (_queueLimit == int.MaxValue)
                return _queue.TryDequeue(out item);

            var result = false;
            try
            { }
            finally // prevent state corrupt while aborting
            {
                if (_queue.TryDequeue(out item))
                {
                    Interlocked.Decrement(ref _counter);
                    result = true;
                }
            }

            return result;
        }

        public bool TryEnqueue(T item)
        {
            if (_queueLimit == int.MaxValue)
            {
                _queue.Enqueue(item);
                return true;
            }

            var result = true;
            try
            { }
            finally
            {
                if (Interlocked.Increment(ref _counter) <= _queueLimit)
                {
                    _queue.Enqueue(item);
                }
                else
                {
                    Interlocked.Decrement(ref _counter);
                    result = false;
                }
            }

            return result;
        }
    }
}
