namespace PrinterApp.Configuration;

public class RandomizerConfiguration
{
    public int MinJobCount { get; set; } = 10;
    public int MaxJobCount { get; set; } = 20;

    public int MinPageCount { get; set; } = 1;
    public int MaxPageCount { get; set; } = 30;

    public int MinDelay { get; init; } = 500;
    public int MaxDelay { get; init; } = 5000;
}
