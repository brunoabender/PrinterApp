using PrinterApp.Configuration;
using PrinterApp.Impl;
using Shouldly;

namespace PrinterApp.Tests.Unit.Impl
{
    public class ProducerTests
    {
        private readonly ApplicationConfiguration _applicationConfiguration;
        public ProducerTests() => _applicationConfiguration = new ApplicationConfiguration() { QueueCapacity = 10, MaxDelay = 10, MaxJobCount = 10, MaxPageCount = 10, NumberOfProducers = 10, MillisecondsPerPage = 10, MinDelay = 10, MinJobCount = 10, MinPageCount = 10 };

        [Fact]
        public async Task Producer_ShouldAddJobsToQueue()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "Produtor1", _applicationConfiguration);

            await producer.RunAsync(CancellationToken.None);

            queue.Count.ShouldBeGreaterThanOrEqualTo(10);
        }

        [Fact]
        public async Task Producer_ShouldStopWhenTokenIsCancelled()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "ProdutorCancelado", _applicationConfiguration);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(100);

            await producer.RunAsync(cts.Token);
            queue.Count.ShouldBeLessThanOrEqualTo(20);
        }

        [Fact]
        public async Task Producer_ShouldNotThrow_WhenCancelled()
        {
            var queue = new FakeQueue();
            var producer = new Producer(queue, "ProdutorSeguro", _applicationConfiguration);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(50);

            await Should.NotThrowAsync(async () => await producer.RunAsync(cts.Token));
        }
    }
}
