using Microsoft.Extensions.Options;
using PrinterApp.Configuration;
using PrinterApp.Impl;

namespace PrinterApp.Core;

public class AppBootstrapper(IOptions<AppConfiguration> options)
{
    private readonly AppConfiguration config = options.Value;

    public async Task RunAsync()
    {
        var queue = new CircularQueue(config.QueueCapacity);
        var printer = new Printer(queue, config.MillisecondsPerPage);
        var cancellationTokenSource = new CancellationTokenSource();

        var producers = new List<Task>();

        for (int i = 0; i < config.NumberOfProducers; i++)
        {
            var producer = new Producer(queue, $"Producer {i + 1}", config.Randomizer);
            producers.Add(producer.RunAsync(cancellationTokenSource.Token));
        }

        var printerTask = printer.RunAsync();

        Console.WriteLine("Digite ENTER para parar a execução.");
        await Task.Run(() =>
        {
            Console.ReadLine();
            Console.WriteLine("[System] Halt solicitado.");
            cancellationTokenSource.Cancel();
            printer.Halt();
        });

        await Task.WhenAll(producers);
        await printerTask;

        Console.WriteLine($"[System] Aplicação finalizada. Itens restantes na fila: {queue.Count}");
    }
}
