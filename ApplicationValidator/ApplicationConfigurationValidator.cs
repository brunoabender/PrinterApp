using Microsoft.Extensions.Options;
using PrinterApp.Configuration;

namespace PrinterApp.ApplicationValidator
{
    public class ApplicationConfigurationValidator : IValidateOptions<ApplicationConfiguration>
    {
        public ValidateOptionsResult Validate(string? name, ApplicationConfiguration applicationConfiguration)
        {
            var failures = new List<string>();

            if (applicationConfiguration.MinJobCount > applicationConfiguration.MaxJobCount)
                failures.Add("MinJobCount não pode ser maior que MaxJobCount.");

            if (applicationConfiguration.MinPageCount > applicationConfiguration.MaxPageCount)
                failures.Add("MinPageCount não pode ser maior que MaxPageCount.");

            if (applicationConfiguration.MinDelay > applicationConfiguration.MaxDelay)
                failures.Add("MinDelay não pode ser maior que MaxDelay.");

            return failures.Count > 0
                ? ValidateOptionsResult.Fail(failures)
                : ValidateOptionsResult.Success;
        }
    }
}
