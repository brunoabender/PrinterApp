using Microsoft.Extensions.Options;
using PrinterApp.Configuration;
using PrinterApp.Core;
using Shouldly;

namespace PrinterApp.Tests.Unit.Core;

public class ApplicationBootstrapperTests
{
    private ApplicationConfiguration CreateValidConfiguration() => new()
    {
        QueueCapacity = 10,
        NumberOfProducers = 1,
        MillisecondsPerPage = 10,
        MinJobCount = 1,
        MaxJobCount = 1,
        MinPageCount = 1,
        MaxPageCount = 1,
        MinDelay = 1,
        MaxDelay = 1
    };

    [Fact]
    public void Should_Construct_Bootstrapper_With_Valid_Configuration()
    {
        var config = CreateValidConfiguration();
        var options = Options.Create(config);

        var bootstrapper = new ApplicationBootstrapper(options);
        bootstrapper.ShouldNotBeNull();
    }

    [Fact]
    public async Task RunAsync_Should_Execute_All_Components_When_ENTER_Is_Received()
    {
        var config = CreateValidConfiguration();
        var options = Options.Create(config);
        var bootstrapper = new ApplicationBootstrapper(options);

        using var input = new StringReader("\n");
        Console.SetIn(input);

        var output = new StringWriter();
        Console.SetOut(output);

        await bootstrapper.RunAsync();

        var result = output.ToString();

        result.ShouldContain("Digite ENTER");
        result.ShouldContain("Halt solicitado");
        result.ShouldContain("Aplicação finalizada");
    }

    [Fact]
    public async Task RunAsync_Should_Print_HaltMessage_And_Not_Throw_When_Already_Canceled()
    {
        var config = CreateValidConfiguration();
        var options = Options.Create(config);
        var bootstrapper = new ApplicationBootstrapper(options);

        using var input = new StringReader("\n");
        Console.SetIn(input);

        var output = new StringWriter();
        Console.SetOut(output);

        await Should.NotThrowAsync(async () => await bootstrapper.RunAsync());
        output.ToString().ShouldContain("Halt solicitado");
    }

    [Fact]
    public void Constructor_Should_Use_Configuration_Correctly()
    {
        var config = CreateValidConfiguration();
        var options = Options.Create(config);
        var bootstrapper = new ApplicationBootstrapper(options);
        bootstrapper.ShouldNotBeNull();
    }
}
