using PrinterApp.Core;
namespace PrinterApp.Impl
{
    public class Printer(IQueue queue, long millisecondsPerPage, CancellationToken token)
    {
        private readonly IQueue _queue = queue;
        private readonly long _millisecondsPerPage = millisecondsPerPage;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private volatile bool _haltRequested = false;

        public async Task RunAsync()
        {
            while (!_haltRequested || !_queue.IsEmpty)
            {
                if (_cancellationTokenSource.IsCancellationRequested && _queue.IsEmpty)
                    break;

                PrintJob? job;

                try
                {
                    job = _queue.Dequeue(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!_cancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine($"[Printer] Imprimindo: {job.Name} ({job.Pages} páginas)");

                    int delay = (int)(job.Pages * _millisecondsPerPage);
                    await Task.Delay(delay);

                    Console.WriteLine($"[Printer] Terminado com sucesso: {job.Name} com tempo de impressão em {delay}");
                }
            }

            Console.WriteLine($"[Printer] Parada de execução com sucesso. Itens na fila: {_queue.Count}");
        }

        public void Halt()
        {
            _haltRequested = true;
            _cancellationTokenSource.Cancel();
        }
    }
}