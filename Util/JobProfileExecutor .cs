using PrinterApp.Configuration;

namespace PrinterApp.Util;

public class JobProfileExecutor(ApplicationConfiguration configuration)
{
    private static readonly ThreadLocal<Random> ThreadSafeRandom = new(() => new Random());
    
    public int NextJobCount() => ThreadSafeRandom.Value!.Next(configuration.MinJobCount, configuration.MaxJobCount);
    public int NextPageCount() => ThreadSafeRandom.Value!.Next(configuration.MinPageCount, configuration.MaxPageCount);
    public int NextDelay() => ThreadSafeRandom.Value!.Next(configuration.MinDelay, configuration.MaxDelay);
}
