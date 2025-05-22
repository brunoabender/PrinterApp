using System.Diagnostics;
using System.Text;
using Shouldly;
using Xunit;

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
            ["AppConfiguration__NumberOfProducers"] = "1",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "5",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "5",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "0",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "0",
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
    public async Task App_ShouldHaltEarly_WithPendingJobsRemaining()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "10",
            ["AppConfiguration__NumberOfProducers"] = "1",
            ["AppConfiguration__RandomizerSettings__MinJobCount"] = "10",
            ["AppConfiguration__RandomizerSettings__MaxJobCount"] = "10",
            ["AppConfiguration__RandomizerSettings__MinPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1",
            ["AppConfiguration__RandomizerSettings__MinDelay"] = "100",
            ["AppConfiguration__RandomizerSettings__MaxDelay"] = "100",
        }, haltAfterMs: 500);

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        process.CancelOutputRead();
        await Task.Delay(100);

        var output = outputBuilder.ToString();
        output.ShouldContain("[System] Halt solicitado", Case.Insensitive);
        output.ShouldContain("Parada de execução", Case.Insensitive);
        error.ShouldBeEmpty();
    }

    [Fact]
    public async Task App_ShouldRespectProducerCancellation()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "5",
            ["AppConfiguration__NumberOfProducers"] = "1",
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

    [Fact]
    public async Task App_ShouldLogBothProducersAndPrinterOutput()
    {
        var (process, outputBuilder) = StartAppWithConfig(new()
        {
            ["AppConfiguration__QueueCapacity"] = "5",
            ["AppConfiguration__NumberOfProducers"] = "2",
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

}