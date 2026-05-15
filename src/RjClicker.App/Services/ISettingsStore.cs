using RjClicker.App.Models;

namespace RjClicker.App.Services;

public interface ISettingsStore
{
    Task SaveAsync(AppSettings settings);
    Task<AppSettings?> LoadAsync();
}
