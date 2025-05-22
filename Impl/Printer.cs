using Microsoft.Extensions.Options;
using PrinterApp.Configuration;
using PrinterApp.Core;
namespace PrinterApp.Impl
{
    public class Printer(IQueue queue, long millisecondsPerPage)
    {
        private readonly IQueue Queue = queue;
        private readonly long MillisecondsPerPage = millisecondsPerPage;
        private readonly CancellationTokenSource CancellationTokenSource = new();
        private volatile bool HaltRequested = false;

        public async Task RunAsync()
        {
            while (!HaltRequested || !Queue.IsEmpty)
            {
                if (CancellationTokenSource.IsCancellationRequested && Queue.IsEmpty)
                    break;

                PrintJob? job;

                try
                {
                    job = Queue.Dequeue(CancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (!CancellationTokenSource.IsCancellationRequested)
                {
                    Console.WriteLine($"[Printer] Imprimindo: {job.Name} ({job.Pages} páginas)");

                    int delay = (int)(job.Pages * MillisecondsPerPage);
                    await Task.Delay(delay);

                    Console.WriteLine($"[Printer] Terminado com sucesso: {job.Name} com tempo de impressão em {delay}");
                }
            }

            Console.WriteLine($"[Printer] Parada de execução com sucesso. Itens na fila: {Queue.Count}");
        }


        public void Halt()
        {
            HaltRequested = true;
            CancellationTokenSource.Cancel();
        }
    }
}