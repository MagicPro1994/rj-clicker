using RjClicker.App.Core.Models;
using RjClicker.App.Core.Services;

namespace RjClicker.App.Core.Sessions;

public sealed class Win32BackgroundClickService : IBackgroundClickService
{
    public Task<BackgroundClickResult> ExecuteClickAsync(PointTarget target, MouseButton button, PressType pressType)
    {
        ArgumentNullException.ThrowIfNull(target);
        // Stub: will be implemented fully in Task 8
        return Task.FromResult(new BackgroundClickResult(Succeeded: true));
    }
}
