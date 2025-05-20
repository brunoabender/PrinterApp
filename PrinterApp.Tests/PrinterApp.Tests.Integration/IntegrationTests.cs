using PrinterApp.Impl;
using Shouldly;

namespace PrinterApp.Tests.Integration
{
    public class IntegrationTests
    {
        [Fact]
        public async Task Console_ShouldOutput_ProducerAndPrinterActivity()
        {
            using var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            var queue = new CircularQueue(2);
            var printer = new Printer(queue);
            var producer1 = new Producer(queue, "Produtor1");

            var cts = new CancellationTokenSource();
            var producerTask = producer1.RunAsync(cts.Token);
            var printerTask = printer.RunAsync();

            await producerTask;
            printer.Halt();
            await printerTask;

            var output = consoleOutput.ToString();

            output.ShouldContain("Produtor1");
            output.ShouldContain("[Printer] Imprimindo");
            output.ShouldContain("Parada de execução");
        }

    }
}

