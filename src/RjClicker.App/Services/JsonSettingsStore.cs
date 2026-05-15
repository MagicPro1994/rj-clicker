using RjClicker.App.Models;
using System.IO;
using System.Text.Json;

namespace RjClicker.App.Services;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly string DefaultSettingsPath =
        Path.Combine(AppContext.BaseDirectory, "settings.json");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _settingsPath;
    private readonly IAppLogger _logger;

    public JsonSettingsStore() : this(DefaultSettingsPath, new FileAppLogger())
    {
    }

    public JsonSettingsStore(string path) : this(path, new FileAppLogger())
    {
    }

    public JsonSettingsStore(string path, IAppLogger logger)
    {
        _settingsPath = path ?? throw new ArgumentNullException(nameof(path));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveAsync(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            var directory = Path.GetDirectoryName(_settingsPath)!;
            Directory.CreateDirectory(directory);

            var json = JsonSerializer.Serialize(settings, SerializerOptions);
            await File.WriteAllTextAsync(_settingsPath, json).ConfigureAwait(false);
        }
        catch (IOException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to save settings", ex).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to save settings", ex).ConfigureAwait(false);
        }
    }

    public async Task<AppSettings?> LoadAsync()
    {
        if (!File.Exists(_settingsPath))
        {
            return null;
        }

        try
        {
            var json = await File.ReadAllTextAsync(_settingsPath).ConfigureAwait(false);
            return JsonSerializer.Deserialize<AppSettings>(json);
        }
        catch (IOException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to load settings", ex).ConfigureAwait(false);
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to load settings", ex).ConfigureAwait(false);
            return null;
        }
        catch (JsonException ex)
        {
            await _logger.LogErrorAsync(nameof(JsonSettingsStore), "Failed to deserialize settings", ex).ConfigureAwait(false);
            return null;
        }
    }
}
