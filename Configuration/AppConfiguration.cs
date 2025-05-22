using PrinterApp.Util;

namespace PrinterApp.Configuration
{
    public class AppConfiguration
    {
        public int QueueCapacity { get; set; } = 50;
        public int NumberOfProducers { get; set; } = 2;
        public RandomizerConfiguration RandomizerSettings { get; set; } = new();
        public Randomizer Randomizer => new(RandomizerSettings);
    }
}
