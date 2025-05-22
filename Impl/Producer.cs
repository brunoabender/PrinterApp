using PrinterApp.Core;
using PrinterApp.Util;

namespace PrinterApp.Impl
{
    public class Producer(IQueue queue, string name, Randomizer random)
    {
        private readonly IQueue Queue = queue;
        private readonly Randomizer Random = random;
        private readonly string Name = name;

        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                int jobs = Random.NextJobCount();
                for (int i = 0; i < jobs && !token.IsCancellationRequested; i++)
                {
                    var job = new PrintJob(RandomFileName.GenerateFileName(), Random.NextPageCount());
                    try
                    {
                        Queue.Enqueue(job);
                        Console.WriteLine($"[{Name}] Produzindo: {job.Name} - com total de {job.Pages} página(s)");
                        await Task.Delay(Random.NextDelay(), token);
                    }
                    catch (FullQueueException ex)
                    {
                        Console.WriteLine($"[{Name}] Erro: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{Name}] Cancelado com segurança.");
            }
        }

    }
}