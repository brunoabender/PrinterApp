using PrinterApp.Core;

namespace PrinterApp.Impl
{
    public class Producer(IQueue queue, string name)
    {
        private readonly IQueue Queue = queue;
        private readonly Random Random = new();
        private readonly string Name = name;

        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                int jobs = Random.Next(10, 20);
                for (int i = 0; i < jobs && !token.IsCancellationRequested; i++)
                {
                    var job = new PrintJob($"Arquivo_{Guid.NewGuid().ToString()[..6]}.txt", Random.Next(60, 80));
                    Queue.Enqueue(job);
                    Console.WriteLine($"[{Name}] Produzindo: {job.Name} - com total de {job.Pages} página(s)");
                    await Task.Delay(Random.Next(500, 5000), token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{Name}] Cancelado com segurança.");
            }
        }

    }
}