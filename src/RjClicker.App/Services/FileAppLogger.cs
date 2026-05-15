using System.Globalization;
using System.IO;
using System.Text;

namespace RjClicker.App.Services;

public sealed class FileAppLogger : IAppLogger
{
    private static readonly string DefaultLogPath = Path.Combine(AppContext.BaseDirectory, "rjclicker.log");

    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly string _logPath;

    public FileAppLogger() : this(DefaultLogPath)
    {
    }

    public FileAppLogger(string logPath)
    {
        _logPath = logPath ?? throw new ArgumentNullException(nameof(logPath));
    }

    public async Task LogErrorAsync(string source, string message, Exception? exception = null)
    {
        var lockAcquired = false;

        try
        {
            await _writeLock.WaitAsync().ConfigureAwait(false);
            lockAcquired = true;

            var directoryPath = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var safeSource = string.IsNullOrWhiteSpace(source) ? "Unknown" : source;
            var safeMessage = message ?? string.Empty;
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

            var lineBuilder = new StringBuilder();
            lineBuilder.Append(timestamp)
                .Append(" ERROR [")
                .Append(safeSource)
                .Append("] ")
                .Append(safeMessage)
                .AppendLine();

            if (exception is not null)
            {
                lineBuilder.Append(exception)
                    .AppendLine();
            }

            await File.AppendAllTextAsync(_logPath, lineBuilder.ToString()).ConfigureAwait(false);
        }
        catch
        {
            // Best-effort logging: suppress all I/O and formatting failures.
        }
        finally
        {
            if (lockAcquired)
            {
                _writeLock.Release();
            }
        }
    }
}
