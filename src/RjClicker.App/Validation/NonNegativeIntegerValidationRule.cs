using System.Globalization;
using System.Windows.Controls;

namespace RjClicker.App.Validation;

public sealed class NonNegativeIntegerValidationRule : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        var text = value?.ToString();
        if (string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult(false, "A value is required.");
        }

        if (!int.TryParse(text, NumberStyles.Integer, cultureInfo, out var parsedValue))
        {
            return new ValidationResult(false, "Enter a whole number.");
        }

        if (parsedValue < 0)
        {
            return new ValidationResult(false, "Value must be 0 or greater.");
        }

        return ValidationResult.ValidResult;
    }
}
