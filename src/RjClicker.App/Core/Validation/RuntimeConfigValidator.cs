using RjClicker.App.Core.Models;

namespace RjClicker.App.Core.Validation;

public sealed record ValidationResult(bool IsValid, IReadOnlyList<string> Errors);

public static class RuntimeConfigValidator
{
    public static ValidationResult Validate(RuntimeConfig config)
    {
        var errors = new List<string>();

        if (config.TotalIntervalMilliseconds < 1)
        {
            errors.Add("Interval must be at least 1 ms");
        }

        if (config.Targets.Count == 0)
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