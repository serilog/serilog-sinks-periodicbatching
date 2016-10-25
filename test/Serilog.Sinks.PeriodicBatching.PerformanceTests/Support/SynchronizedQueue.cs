using System.Collections.Generic;

namespace Serilog.Sinks.PeriodicBatching.PerformanceTests.Support
{
    class SynchronizedQueue<T> : Queue<T>
    {
        const int NON_BOUNDED = -1;

        readonly int _queueLimit;

        public SynchronizedQueue()
        {
            _queueLimit = NON_BOUNDED;
        }

        public SynchronizedQueue(int queueLimit)
        {
            _queueLimit = queueLimit;
        }

        public bool TryDequeue(out T item)
        {
            item = default(T);

            lock (this)
            {
                if (base.Count > 0)
                {
                    item = base.Dequeue();
                    return true;
                }

                return false;
            }
        }

        public bool TryEnqueue(T item)
        {
            lock (this)
            {
                if (base.Count < _queueLimit || _queueLimit == NON_BOUNDED)
                {
                    base.Enqueue(item);
                    return true;
                }

                return false;
            }
        }
    }
}
