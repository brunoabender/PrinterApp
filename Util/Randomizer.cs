using PrinterApp.Configuration;

namespace PrinterApp.Util;

public class Randomizer(RandomizerConfiguration? config = null)
{
    private static readonly ThreadLocal<Random> ThreadSafeRandom = new(() => new Random());
    private readonly RandomizerConfiguration config = config ?? new RandomizerConfiguration();

    public int NextJobCount() => ThreadSafeRandom.Value!.Next(config.MinJobCount, config.MaxJobCount);
    public int NextPageCount() => ThreadSafeRandom.Value!.Next(config.MinPageCount, config.MaxPageCount);
    public int NextDelay() => ThreadSafeRandom.Value!.Next(config.MinDelay, config.MaxDelay);
}
