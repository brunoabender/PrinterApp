using PrinterApp.Core;

namespace PrinterApp.Impl
{
    public class CircularQueue(int capacity) : IQueue
    {
        private readonly Queue<PrintJob> _queue = new();
        private readonly int _capacity = capacity;
        private readonly object _lock = new();
        private readonly int _timeToCheck = 100;

        public bool IsEmpty
        {
            get
            {
                lock (_lock) return _queue.Count == 0;
            }
        }

        public void Enqueue(PrintJob job)
        {
            lock (_lock)
            {
                if (_queue.Count >= _capacity)
                    throw new FullQueueException($"A fila de impressão está cheia. Não foi possivel colocar {job.Name}");

                _queue.Enqueue(job);
                Monitor.PulseAll(_lock);
            }
        }

        public PrintJob Dequeue(CancellationToken token)
        {
            lock (_lock)
            {
                while (_queue.Count == 0)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();

                    Monitor.Wait(_lock, TimeSpan.FromMilliseconds(_timeToCheck));
                }

                var job = _queue.Dequeue();
                Monitor.PulseAll(_lock);
                return job;
            }
        }

        public int Count
        {
            get
            {
                lock (_lock) return _queue.Count;
            }
        }
    }
}
