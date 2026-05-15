namespace RjClicker.App.Services;

public interface IAppLogger
{
    Task LogErrorAsync(string source, string message, Exception? exception = null);
}
