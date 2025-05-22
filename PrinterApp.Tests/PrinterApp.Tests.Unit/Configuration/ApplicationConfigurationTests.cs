using PrinterApp.ApplicationValidator;
using PrinterApp.Configuration;
using Shouldly;
using System.ComponentModel.DataAnnotations;

namespace PrinterApp.Tests.Unit.Configuration;

public class ApplicationConfigurationTests
{
    private static IList<ValidationResult> ValidateWithDataAnnotations(ApplicationConfiguration config)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(config);
        Validator.TryValidateObject(config, context, results, true);
        return results;
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
}
