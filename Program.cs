using PrinterApp.Impl;

var queue = new CircularQueue(10);
var printer = new Printer(queue);
var cancellationTokenSource = new CancellationTokenSource();

var firstProducer = new Producer(queue, "Primeiro Producer");
var secoundProducer = new Producer(queue, "Segundo Producer");

var firstProducerResult = firstProducer.RunAsync(cancellationTokenSource.Token);
var secondProducerResult = secoundProducer.RunAsync(cancellationTokenSource.Token);
var printerTask = printer.RunAsync();

var haltSource = new TaskCompletionSource();
var inputTask = MonitorUserInputAsync(haltSource);

await Task.WhenAny(Task.WhenAll(firstProducerResult, secondProducerResult), haltSource.Task);

if (haltSource.Task.IsCompleted)
{
    cancellationTokenSource.Cancel();       
    printer.Halt();     
}
else
    Console.WriteLine("[System] Producers concluídos. Aguardando a impressora esvaziar a fila...");

await printerTask;

Console.WriteLine($"[System] Encerrado com sucesso. Itens restantes na fila: {queue.Count}");

static async Task MonitorUserInputAsync(TaskCompletionSource haltSignal)
{
    Console.WriteLine("Digite ENTER a qualquer momento para parar a execução.");
    await Task.Run(() => Console.ReadLine());
    Console.WriteLine("[System] Halt solicitado pelo usuário.");
    haltSignal.TrySetResult();
}