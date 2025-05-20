using PrinterApp.Core;
using Shouldly;

namespace PrinterApp.Tests.Unit.Core
{
    public class PrintJobTests
    {
        [Fact]
        public void PrintJob_ShouldStoreNameAndPages()
        {
            var job = new PrintJob("DocumentoA", 5);

            job.Name.ShouldBe("DocumentoA");
            job.Pages.ShouldBe(5);
        }

        [Fact]
        public void PrintJob_ShouldBeEqual_WhenPropertiesAreEqual()
        {
            var job1 = new PrintJob("DocumentoB", 10);
            var job2 = new PrintJob("DocumentoB", 10);

            job1.ShouldBe(job2);
        }

        [Fact]
        public void PrintJob_ShouldNotBeEqual_WhenPropertiesDiffer()
        {
            var job1 = new PrintJob("Doc1", 1);
            var job2 = new PrintJob("Doc2", 1);

            job1.ShouldNotBe(job2);
        }

        [Fact]
        public void PrintJob_ShouldBeImmutable()
        {
            var job = new PrintJob("Doc", 2);
            job.ShouldBeOfType<PrintJob>();
        }
    }
}
