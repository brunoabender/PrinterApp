using PrinterApp.Core;
using Shouldly;

namespace PrinterApp.Tests.Unit.Core
{
    public class FullQueueExceptionTests
    {
        [Fact]
        public void FullQueueException_ShouldInheritFromException()
        {
            var ex = new FullQueueException("fila cheia");
            ex.ShouldBeOfType<FullQueueException>();
        }

        [Fact]
        public void FullQueueException_ShouldContainCustomMessage()
        {
            var message = "A fila está cheia para o trabalho X";
            var ex = new FullQueueException(message);
            ex.Message.ShouldBe(message);
        }
    }
}
