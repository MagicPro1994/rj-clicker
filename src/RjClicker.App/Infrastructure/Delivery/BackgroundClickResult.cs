namespace RjClicker.App.Infrastructure.Delivery;

public sealed record BackgroundClickResult(bool Succeeded, string? WarningMessage = null);