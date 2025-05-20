using PrinterApp.Core;

namespace PrinterApp.Impl
{
    public class CircularQueue(int capacity) : IQueue
    {
        private readonly Queue<PrintJob> Queue = new();
        private readonly int Capacity = capacity;
        private readonly object Lock = new();
        private readonly int TimeToCheck = 100;

        public bool IsEmpty
        {
            get
            {
                lock (Lock) return Queue.Count == 0;
            }
        }

        public void Enqueue(PrintJob job)
        {
            lock (Lock)
            {
                if (Queue.Count >= Capacity)
                    throw new FullQueueException($"A fila de impressão está cheia. Não foi possivel colocar {job.Name}");

                Queue.Enqueue(job);
                Monitor.PulseAll(Lock);
            }
        }

        public PrintJob Dequeue(CancellationToken token)
        {
            lock (Lock)
            {
                while (Queue.Count == 0)
                {
                    if (token.IsCancellationRequested)
                        throw new OperationCanceledException();

                    Monitor.Wait(Lock, TimeSpan.FromMilliseconds(TimeToCheck));
                }

                var job = Queue.Dequeue();
                Monitor.PulseAll(Lock);
                return job;
            }
        }

        public int Count
        {
            get
            {
                lock (Lock) return Queue.Count;
            }
        }
    }
}
