using RjClicker.App.Models;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace RjClicker.App.Services;

public sealed class JsonSettingsStore : ISettingsStore
{
    private static readonly string DefaultSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "RjClicker",
        "settings.json");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    private readonly string _settingsPath;

    public JsonSettingsStore() : this(DefaultSettingsPath)
    {
    }

    public JsonSettingsStore(string path)
    {
        _settingsPath = path ?? throw new ArgumentNullException(nameof(path));
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
            Debug.WriteLine($"[JsonSettingsStore] Failed to save settings: {ex.Message}");
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
            Debug.WriteLine($"[JsonSettingsStore] Failed to load settings: {ex.Message}");
            return null;
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"[JsonSettingsStore] Failed to deserialize settings: {ex.Message}");
            return null;
        }
    }
}
