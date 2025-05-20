namespace PrinterApp.Core
{
    public interface IQueue
    {
        void Enqueue(PrintJob job);
        PrintJob Dequeue(CancellationToken token);
        bool IsEmpty { get; }
        int Count { get; }
    }
}


