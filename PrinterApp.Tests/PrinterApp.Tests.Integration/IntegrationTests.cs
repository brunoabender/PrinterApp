using System.Diagnostics;
using System.Text;
using Shouldly;

namespace PrinterApp.Tests.Integration;

public class AppExecutableTests
{
    [Fact]
    public async Task App_ShouldOutput_ExpectedConsoleMessages_WithInjectedConfiguration()
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

        var env = processStartInfo.EnvironmentVariables;
        env["AppConfiguration__QueueCapacity"] = "2";
        env["AppConfiguration__NumberOfProducers"] = "1";
        env["AppConfiguration__RandomizerSettings__MinJobCount"] = "1";
        env["AppConfiguration__RandomizerSettings__MaxJobCount"] = "1";
        env["AppConfiguration__RandomizerSettings__MinPageCount"] = "1";
        env["AppConfiguration__RandomizerSettings__MaxPageCount"] = "1";
        env["AppConfiguration__RandomizerSettings__MinDelay"] = "10";
        env["AppConfiguration__RandomizerSettings__MaxDelay"] = "10";

        var outputBuilder = new StringBuilder();
        using var process = new Process { StartInfo = processStartInfo };

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
                        await process.StandardInput.WriteLineAsync();
                        await process.StandardInput.FlushAsync();
                    }
                    catch (IOException) { }
                }
            }
        };

        process.Start();
        process.BeginOutputReadLine();

        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        var output = outputBuilder.ToString();

        output.Contains("Digite ENTER").ShouldBeTrue("esperava ver o prompt de parada");
        output.Contains("[Printer] Imprimindo").ShouldBeTrue("esperava ver mensagens de impressão");
        output.Contains("[System] Aplicação finalizada").ShouldBeTrue("esperava ver encerramento do sistema");
        error.ShouldBeEmpty("esperava que não houvesse erros no console");
    }
}
