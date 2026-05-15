namespace RjClicker.App.Services;

public sealed class AppExceptionLogger
{
    private readonly IAppLogger _logger;

    public AppExceptionLogger(IAppLogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task LogDispatcherUnhandledExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unhandled dispatcher exception", exception);
    }

    public Task LogUnhandledExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unhandled app domain exception", exception);
    }

    public Task LogUnobservedTaskExceptionAsync(Exception exception)
    {
        return _logger.LogErrorAsync("App", "Unobserved task exception", exception);
    }
}
