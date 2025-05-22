using Microsoft.Extensions.Options;
using PrinterApp.Configuration;
using PrinterApp.Impl;

namespace PrinterApp.Core;

public class ApplicationBootstrapper(IOptions<ApplicationConfiguration> options)
{
    private readonly ApplicationConfiguration _config = options.Value;

    public async Task RunAsync()
    {
        var queue = new CircularQueue(_config.QueueCapacity);
        var cancellationTokenSource = new CancellationTokenSource();
        var printer = new Printer(queue, _config.MillisecondsPerPage, cancellationTokenSource.Token);

        var producers = new List<Task>();

        for (int i = 0; i < _config.NumberOfProducers; i++)
        {
            var producer = new Producer(queue, $"Producer {i + 1}", _config);
            producers.Add(producer.RunAsync(cancellationTokenSource.Token));
        }

        Console.Out.Flush();

        var printerTask = printer.RunAsync();

        await Task.Run(() =>
        {
            Console.WriteLine("Digite ENTER para parar a execução.");
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
