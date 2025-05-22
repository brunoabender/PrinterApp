using PrinterApp.Core;
using PrinterApp.Impl;
using Shouldly;

namespace PrinterApp.Tests.Unit.Impl
{
    public class FakeQueue : IQueue
    {
        private readonly Queue<PrintJob> Jobs = new();
        private readonly object Lock = new();
        private readonly int TimeToCheck = 20;

        public void Enqueue(PrintJob job)
        {
            lock (Lock)
            {
                Jobs.Enqueue(job);
                Monitor.PulseAll(Lock); 
            }
        }

        public PrintJob Dequeue(CancellationToken token)
        {
            lock (Lock)
            {
                while (Jobs.Count == 0)
                {
                    Monitor.Wait(Lock, TimeSpan.FromMilliseconds(TimeToCheck));

                    if (token.IsCancellationRequested)
                    {
                        Monitor.PulseAll(Lock);
                        throw new OperationCanceledException();
                    }
                }

                var job = Jobs.Dequeue();
                Monitor.PulseAll(Lock); 
                return job;
            }
        }

        public bool IsEmpty
        {
            get
            {
                lock (Lock)
                {
                    return Jobs.Count == 0;
                }
            }
        }

        public int Count
        {
            get
            {
                lock (Lock)
                {
                    return Jobs.Count;
                }
            }
        }
    }


    public class PrinterTests
    {
        [Fact]
        public async Task Printer_ShouldPrintAllJobsInQueue()
        {
            var queue = new FakeQueue();
            queue.Enqueue(new PrintJob("Doc1", 1));
            queue.Enqueue(new PrintJob("Doc2", 2));

            var printer = new Printer(queue, 250);
            var task = printer.RunAsync();
            printer.Halt();
            await task;

            queue.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public async Task Printer_ShouldStopGracefully_WhenHaltIsCalledEarly()
        {
            var queue = new FakeQueue();
            queue.Enqueue(new PrintJob("Job1", 10));

            var printer = new Printer(queue, 250);
            var task = printer.RunAsync();
            printer.Halt();
            await task;

            queue.Count.ShouldBeLessThanOrEqualTo(1);
        }

        [Fact]
        public async Task Printer_ShouldNotThrow_WhenQueueHasItemAndHaltRequested()
        {
            var queue = new FakeQueue();
            var printer = new Printer(queue, 250);

            queue.Enqueue(new PrintJob("Test", 1));

            var task = printer.RunAsync();
            printer.Halt();

            await Should.NotThrowAsync(async () => await task);
        }
    }
}
