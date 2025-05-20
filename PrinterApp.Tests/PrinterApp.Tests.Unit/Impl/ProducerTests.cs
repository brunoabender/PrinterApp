using PrinterApp.Impl;
using Shouldly;

namespace PrinterApp.Tests.Unit.Impl
{
    public class ProducerTests
    {
        //Esse teste pode demorar

        [Fact]
        public async Task Producer_ShouldAddJobsToQueue()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "Produtor1");

            await producer.RunAsync(CancellationToken.None);

            queue.Count.ShouldBeGreaterThanOrEqualTo(10);
            queue.Count.ShouldBeLessThanOrEqualTo(20);
        }

        [Fact]
        public async Task Producer_ShouldStopWhenTokenIsCancelled()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "ProdutorCancelado");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(100); 

            await producer.RunAsync(cts.Token);
            queue.Count.ShouldBeLessThanOrEqualTo(20);
        }

        [Fact]
        public async Task Producer_ShouldNotThrow_WhenCancelled()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "ProdutorSeguro");

            var cts = new CancellationTokenSource();
            cts.CancelAfter(50); 

            await Should.NotThrowAsync(async () => await producer.RunAsync(cts.Token));
        }
    }
}
