using System.ComponentModel.DataAnnotations;

namespace PrinterApp.Configuration
{
    public record ApplicationConfiguration
    {
        [Range(1, int.MaxValue)]
        public int QueueCapacity { get; init; }

        [Range(1, int.MaxValue)]
        public int NumberOfProducers { get; init; }

        [Range(1, int.MaxValue)]
        public int MillisecondsPerPage { get; init; }

        [Range(1, int.MaxValue)]
        public int MinJobCount { get; init; }

        [Range(1, int.MaxValue)]
        public int MaxJobCount { get; init; }

        [Range(1, int.MaxValue)]
        public int MinPageCount { get; init; }

        [Range(1, int.MaxValue)]
        public int MaxPageCount { get; init; }

        [Range(1, int.MaxValue)]
        public int MinDelay { get; init; }

        [Range(1, int.MaxValue)]
        public int MaxDelay { get; init; }
    }
}
