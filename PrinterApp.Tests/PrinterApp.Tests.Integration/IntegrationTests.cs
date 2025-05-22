using PrinterApp.ApplicationValidator;
using PrinterApp.Configuration;
using Shouldly;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace PrinterApp.Tests.Integration;

public class AppExecutableTests
{
    [Fact]
    public async Task App_ShouldOutput_ExpectedConsoleMessages_WithInjectedConfiguration()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "2",
            ["AppConfiguration__NumberOfProducers"] = "1",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "10",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "10",
        });

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();

        output.Contains("Digite ENTER").ShouldBeTrue("esperava ver o prompt de parada");
        output.Contains("[Printer] Imprimindo").ShouldBeTrue("esperava ver mensagens de impressão");
        output.Contains("[System] Aplicação finalizada").ShouldBeTrue("esperava ver encerramento do sistema");
        error.ShouldBeEmpty("esperava que não houvesse erros no console");
    }

    [Fact]
    public async Task App_ShouldPrintAllJobs_BeforeShutdown()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "5",
            ["AppConfiguration__NumberOfProducers"] = "1",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "3",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "3",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "10",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "10",
        });

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();
        output.ShouldContain("[Printer] Imprimindo", Case.Insensitive);
        output.ShouldContain("[System] Aplicação finalizada");
        output.Count(c => c == '[').ShouldBeGreaterThanOrEqualTo(3, "esperava ao menos 3 mensagens de impressão ou log do sistema");
        error.ShouldBeEmpty();
    }

    [Fact]
    public async Task App_ShouldRespectQueueCapacity()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "1",
            ["AppConfiguration__NumberOfProducers"] = "2",
            ["AppConfiguration__MillisecondsPerPage"] = "1000",
            ["AppConfiguration__MinJobCount"] = "10",
            ["AppConfiguration__MaxJobCount"] = "10",
            ["AppConfiguration__MinDelay"] = "10",
            ["AppConfiguration__MaxDelay"] = "100",
        });

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();
        output.ShouldContain("[Printer]", Case.Insensitive);
        output.ShouldContain("[System] Aplicação finalizada");
        output.ShouldContain("fila de impressão está cheia", Case.Insensitive);
        output.Split("\n").Count(l => l.Contains("[Printer] Imprimindo")).ShouldBe(1);
        error.ShouldBeEmpty();
    }


    [Fact]
    public async Task App_ShouldRespectProducerCancellation()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "5",
            ["AppConfiguration__NumberOfProducers"] = "1",
            ["AppConfiguration__MillisecondsPerPage"] = "50",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "20",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "20",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "2000",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "2000",
        }, haltAfterMs: 500);

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();

        output.ShouldContain("Cancelado com segurança", Case.Insensitive);
        output.ShouldContain("[System] Aplicação finalizada");
        error.ShouldBeEmpty();
    }

    [Fact]
    public async Task App_ShouldLogBothProducersAndPrinterOutput()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "5",
            ["AppConfiguration__NumberOfProducers"] = "2",
            ["AppConfiguration__MillisecondsPerPage"] = "50",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "3",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "2",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "10",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "100",
        });

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();

        output.ShouldContain("[Producer 1]", Case.Insensitive);
        output.ShouldContain("[Producer 2]", Case.Insensitive);
        output.ShouldContain("[Printer] Imprimindo", Case.Insensitive);
        output.ShouldContain("[System] Aplicação finalizada", Case.Insensitive);
        error.ShouldBeEmpty();
    }


    [Fact]
    public void Should_Validate_CorrectConfiguration()
    {
        var config = new ApplicationConfiguration
        {
            QueueCapacity = 10,
            NumberOfProducers = 2,
            MillisecondsPerPage = 100,
            MinJobCount = 1,
            MaxJobCount = 5,
            MinPageCount = 10,
            MaxPageCount = 20,
            MinDelay = 100,
            MaxDelay = 300
        };

        var results = ValidateWithDataAnnotations(config);
        results.ShouldBeEmpty();

        var validator = new ApplicationConfigurationValidator();
        var logicResult = validator.Validate(null, config);
        logicResult.Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Should_Invalidate_When_AnyFieldIsZero()
    {
        var config = new ApplicationConfiguration
        {
            QueueCapacity = 0,
            NumberOfProducers = 0,
            MillisecondsPerPage = 0,
            MinJobCount = 0,
            MaxJobCount = 0,
            MinPageCount = 0,
            MaxPageCount = 0,
            MinDelay = 0,
            MaxDelay = 0
        };

        var results = ValidateWithDataAnnotations(config);
        results.Count.ShouldBeGreaterThan(0);
        results.ShouldContain(r => r.MemberNames.Contains(nameof(ApplicationConfiguration.QueueCapacity)));
        results.ShouldContain(r => r.MemberNames.Contains(nameof(ApplicationConfiguration.MinJobCount)));
    }

    [Fact]
    public void Should_Invalidate_Logical_Mismatch_MinJobCount_Greater_Than_MaxJobCount()
    {
        var config = new ApplicationConfiguration
        {
            QueueCapacity = 10,
            NumberOfProducers = 1,
            MillisecondsPerPage = 100,
            MinJobCount = 10,
            MaxJobCount = 5,
            MinPageCount = 5,
            MaxPageCount = 10,
            MinDelay = 100,
            MaxDelay = 200
        };

        var validator = new ApplicationConfigurationValidator();
        var result = validator.Validate(null, config);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("MinJobCount não pode ser maior que MaxJobCount.");
    }

    [Fact]
    public void Should_Invalidate_Logical_Mismatch_MinPageCount_Greater_Than_MaxPageCount()
    {
        var config = new ApplicationConfiguration
        {
            QueueCapacity = 10,
            NumberOfProducers = 1,
            MillisecondsPerPage = 100,
            MinJobCount = 1,
            MaxJobCount = 5,
            MinPageCount = 50,
            MaxPageCount = 20,
            MinDelay = 100,
            MaxDelay = 200
        };

        var validator = new ApplicationConfigurationValidator();
        var result = validator.Validate(null, config);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("MinPageCount não pode ser maior que MaxPageCount.");
    }

    [Fact]
    public void Should_Invalidate_Logical_Mismatch_MinDelay_Greater_Than_MaxDelay()
    {
        var config = new ApplicationConfiguration
        {
            QueueCapacity = 10,
            NumberOfProducers = 1,
            MillisecondsPerPage = 100,
            MinJobCount = 1,
            MaxJobCount = 5,
            MinPageCount = 10,
            MaxPageCount = 20,
            MinDelay = 500,
            MaxDelay = 100
        };

        var validator = new ApplicationConfigurationValidator();
        var result = validator.Validate(null, config);
        result.Failed.ShouldBeTrue();
        result.Failures.ShouldContain("MinDelay não pode ser maior que MaxDelay.");
    }

    [Fact]
    public void Should_Fail_When_Required_Fields_Are_Missing()
    {
        var config = new ApplicationConfiguration();
        var results = ValidateWithDataAnnotations(config);
        results.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Should_Integrate_With_Options_Validation_Pipeline()
    {
        var validator = new ApplicationConfigurationValidator();

        var validConfig = new ApplicationConfiguration
        {
            QueueCapacity = 100,
            NumberOfProducers = 2,
            MillisecondsPerPage = 10,
            MinJobCount = 1,
            MaxJobCount = 5,
            MinPageCount = 1,
            MaxPageCount = 10,
            MinDelay = 100,
            MaxDelay = 200
        };

        var validationResult = validator.Validate("AppConfiguration", validConfig);
        validationResult.Succeeded.ShouldBeTrue();

        var invalidConfig = validConfig with { MinDelay = 300, MaxDelay = 100 };
        var invalidResult = validator.Validate("AppConfiguration", invalidConfig);
        invalidResult.Failed.ShouldBeTrue();
        invalidResult.Failures.ShouldContain("MinDelay não pode ser maior que MaxDelay.");
    }


    private static List<ValidationResult> ValidateWithDataAnnotations(ApplicationConfiguration config)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(config);
        Validator.TryValidateObject(config, context, results, true);
        return results;
    }

    private static (Process Process, StringBuilder OutputBuilder) StartAppWithConfig(Dictionary<string, string> envVars, int? haltAfterMs = null)
    {
        var appPath = typeof(Program).Assembly.Location;
        File.Exists(appPath).ShouldBeTrue("PrinterApp.dll não foi encontrado no caminho esperado.");

        var processStartInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"\"{appPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var kv in envVars)
        {
            processStartInfo.EnvironmentVariables[kv.Key] = kv.Value;
        }

        var outputBuilder = new StringBuilder();
        var process = new Process { StartInfo = processStartInfo };

        process.OutputDataReceived += async (sender, args) =>
        {
            if (args.Data != null)
            {
                Console.WriteLine("[APP] " + args.Data);
                outputBuilder.AppendLine(args.Data);

                if (args.Data.Contains("Digite ENTER", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        if (haltAfterMs.HasValue)
                        {
                            await Task.Delay(haltAfterMs.Value);
                        }
                        await process.StandardInput.WriteLineAsync();
                        await process.StandardInput.FlushAsync();
                    }
                    catch (IOException) { }
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();

        return (process, outputBuilder);
    }
}