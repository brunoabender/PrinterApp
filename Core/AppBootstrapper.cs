using Microsoft.Extensions.Options;
using PrinterApp.Configuration;
using PrinterApp.Impl;

namespace PrinterApp.Core;

public class AppBootstrapper(IOptions<AppConfiguration> options)
{
    private readonly AppConfiguration Config = options.Value;

    public async Task RunAsync()
    {
        var queue = new CircularQueue(Config.QueueCapacity);
        var cancellationTokenSource = new CancellationTokenSource();
        var printer = new Printer(queue, Config.MillisecondsPerPage, cancellationTokenSource.Token);

        var producers = new List<Task>();

        for (int i = 0; i < Config.NumberOfProducers; i++)
        {
            var producer = new Producer(queue, $"Producer {i + 1}", Config.Randomizer);
            producers.Add(producer.RunAsync(cancellationTokenSource.Token));
        }

        var printerTask = printer.RunAsync();

        Console.WriteLine("Digite ENTER para parar a execução.");
        await Task.Run(() =>
        {
            Console.ReadLine();
            Console.WriteLine("[System] Halt solicitado.");
            printer.Halt();
            cancellationTokenSource.Cancel();
        });

        try
        {
            await Task.WhenAll(producers);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[System] Producers cancelados com segurança.");
        }

        await printerTask;

        Console.WriteLine($"[System] Aplicação finalizada. Itens restantes na fila: {queue.Count}");
    }
}
