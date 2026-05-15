using RjClicker.App.Core.Models;
using RjClicker.App.Infrastructure.Delivery;

namespace RjClicker.App.Core.Sessions;

public interface IClickDispatcher
{
    Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken);
}

public sealed class ClickDispatcher : IClickDispatcher
{
    private readonly IForegroundClickService _foregroundClickService;
    private readonly IBackgroundClickService _backgroundClickService;

    public ClickDispatcher(
        IForegroundClickService foregroundClickService,
        IBackgroundClickService backgroundClickService)
    {
        _foregroundClickService = foregroundClickService ?? throw new ArgumentNullException(nameof(foregroundClickService));
        _backgroundClickService = backgroundClickService ?? throw new ArgumentNullException(nameof(backgroundClickService));
    }

    public async Task DispatchAsync(RuntimeConfig config, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(config);

        // Route by delivery mode
        if (config.DeliveryMode == DeliveryMode.Foreground)
        {
            await ExecuteForegroundClick(config, cancellationToken);
        }
        else if (config.DeliveryMode == DeliveryMode.Background)
        {
            await ExecuteBackgroundClick(config, cancellationToken);
        }
        else
        {
            throw new InvalidOperationException($"Unknown delivery mode: {config.DeliveryMode}");
        }
    }

    private async Task ExecuteForegroundClick(RuntimeConfig config, CancellationToken cancellationToken)
    {
        // For now, execute click on first target if available
        // This will be expanded in Task 6 to handle all targets based on ClickMode
        if (config.Targets.Count > 0)
        {
            var target = config.Targets[0];
            await _foregroundClickService.ExecuteClickAsync(target, config.MouseButton, config.PressType);
        }
    }

    private async Task ExecuteBackgroundClick(RuntimeConfig config, CancellationToken cancellationToken)
    {
        // For now, execute click on first target if available
        // This will be expanded in Task 6 to handle all targets based on ClickMode
        if (config.Targets.Count > 0)
        {
            var target = config.Targets[0];
            var result = await _backgroundClickService.ExecuteClickAsync(target, config.MouseButton, config.PressType);

            // Log warning if background click failed, but don't throw
            if (!result.Succeeded && !string.IsNullOrEmpty(result.WarningMessage))
            {
                // Warning will be logged in Task 7 (logging integration)
            }
        }
    }
}
