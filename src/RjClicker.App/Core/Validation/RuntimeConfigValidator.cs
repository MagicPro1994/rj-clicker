using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Validation;

public sealed class ValidationResult
{
    private static readonly IReadOnlyList<string> EmptyErrors = Array.Empty<string>();

    public ValidationResult(bool isValid, IEnumerable<string>? errors)
    {
        IsValid = isValid;
        Errors = errors is null ? EmptyErrors : Array.AsReadOnly(errors.ToArray());
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }
}

public static class RuntimeConfigValidator
{
    public static ValidationResult Validate(RuntimeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = new List<string>();

        if (config.TotalIntervalMilliseconds < 1)
        {
            errors.Add("Interval must be at least 1 ms");
        }

        if (config.Targets is null || config.Targets.Count == 0)
        {
            errors.Add("Config must include at least one target");
        }

        if (config.UseCounter && (!config.MaxClicks.HasValue || config.MaxClicks.Value < 1))
        {
            errors.Add("Counter requires MaxClicks >= 1");
        }

        return new ValidationResult(errors.Count == 0, errors);
    }
}