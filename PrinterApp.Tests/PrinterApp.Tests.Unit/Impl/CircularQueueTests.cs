using PrinterApp.Core;
using PrinterApp.Impl;
using Shouldly;

namespace PrinterApp.Tests.Unit.Impl
{
    public class CircularQueueTests
    {
        [Fact]
        public void Enqueue_ShouldAddJob_WhenQueueNotFull()
        {
            var queue = new CircularQueue(5);
            var job = new PrintJob("Job1", 3);

            queue.Enqueue(job);

            queue.Count.ShouldBe(1);
            queue.IsEmpty.ShouldBeFalse();
        }

        [Fact]
        public void Enqueue_ShouldThrowFullQueueException_WhenQueueIsFull()
        {
            var queue = new CircularQueue(1);
            queue.Enqueue(new PrintJob("Job1", 3));

            var exception = Should.Throw<FullQueueException>(() =>
                queue.Enqueue(new PrintJob("Job2", 2))
            );

            exception.Message.ShouldContain("fila de impressão está cheia");
        }

        [Fact]
        public void Dequeue_ShouldReturnJob_WhenQueueHasItems()
        {
            var queue = new CircularQueue(5);
            var job = new PrintJob("Job1", 3);
            queue.Enqueue(job);

            var result = queue.Dequeue(CancellationToken.None);

            result.ShouldBe(job);
            queue.Count.ShouldBe(0);
        }

        [Fact]
        public void Dequeue_ShouldBlockUntilItemAvailable_ThenReturn()
        {
            var queue = new CircularQueue(1);
            var cts = new CancellationTokenSource();
            PrintJob? result = null;

            var dequeueTask = Task.Run(() =>
            {
                result = queue.Dequeue(cts.Token);
            });

            Task.Delay(200)?.Wait();
            queue.Enqueue(new PrintJob("DelayedJob", 2));
            dequeueTask?.Wait(1000);

            result.ShouldNotBeNull();
            result!.Name.ShouldBe("DelayedJob");
        }

        [Fact]
        public void Dequeue_ShouldThrowOperationCanceled_WhenTokenIsCancelled()
        {
            var queue = new CircularQueue(1);
            var cts = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                Assert.Throws<OperationCanceledException>(() =>
                {
                    queue.Dequeue(cts.Token);
                });
            });

            Task.Delay(200)?.Wait();
            cts.Cancel();
            task?.Wait(1000);
        }
    }
}
