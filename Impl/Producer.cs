using PrinterApp.Core;
using PrinterApp.Util;

namespace PrinterApp.Impl
{
    public class Producer(IQueue queue, string name, Randomizer random)
    {
        private readonly IQueue Queue = queue;
        private readonly Randomizer random = random;
        private readonly string Name = name;

        public async Task RunAsync(CancellationToken token)
        {
            try
            {
                int jobs = random.NextJobCount();
                for (int i = 0; i < jobs && !token.IsCancellationRequested; i++)
                {
                    var job = new PrintJob(RandomFileName.GenerateFileName(), random.NextPageCount());
                    Queue.Enqueue(job);
                    Console.WriteLine($"[{Name}] Produzindo: {job.Name} - com total de {job.Pages} página(s)");
                    await Task.Delay(random.NextDelay(), token);
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[{Name}] Cancelado com segurança.");
            }
        }

    }
}