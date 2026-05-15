using Microsoft.Extensions.DependencyInjection;
using RjClicker.App.Models;
using RjClicker.App.Services;
using RjClicker.App.ViewModels;
using System.Windows;
using Key = System.Windows.Input.Key;
using ModifierKeys = System.Windows.Input.ModifierKeys;

namespace RjClicker.App;

public partial class App : Application
{
	private IServiceProvider? _serviceProvider;

	protected override async void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		_serviceProvider = ServiceRegistration.BuildServiceProvider();
		var settingsStore = _serviceProvider.GetRequiredService<ISettingsStore>();
		var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();

		var settings = await settingsStore.LoadAsync();
		if (settings != null)
		{
			ApplySettingsToViewModel(viewModel, settings);
		}

		var mainWindow = new MainWindow(viewModel);
		mainWindow.Show();
	}

	protected override async void OnExit(ExitEventArgs e)
	{
		if (_serviceProvider != null)
		{
			var viewModel = _serviceProvider.GetRequiredService<MainViewModel>();
			var store = _serviceProvider.GetRequiredService<ISettingsStore>();
			await store.SaveAsync(ExtractSettingsFromViewModel(viewModel));
			(_serviceProvider as IDisposable)?.Dispose();
		}

		base.OnExit(e);
	}

	private static void ApplySettingsToViewModel(MainViewModel viewModel, AppSettings settings)
	{
		if (Enum.TryParse<Core.Models.MouseButton>(settings.MouseButton, out var mouseButton))
			viewModel.SelectedMouseButton = mouseButton;

		if (Enum.TryParse<Core.Models.PressType>(settings.PressType, out var pressType))
			viewModel.SelectedPressType = pressType;

		if (Enum.TryParse<Core.Models.ClickMode>(settings.ClickMode, out var clickMode))
			viewModel.SelectedClickMode = clickMode;

		if (Enum.TryParse<Core.Models.DeliveryMode>(settings.DeliveryMode, out var deliveryMode))
			viewModel.SelectedDeliveryMode = deliveryMode;

		viewModel.UseCounter = settings.UseCounter;
		viewModel.MaxClicks = settings.MaxClicks;
		viewModel.HoursPart = settings.HoursPart;
		viewModel.MinutesPart = settings.MinutesPart;
		viewModel.SecondsPart = settings.SecondsPart;
		viewModel.TenthsPart = settings.TenthsPart;
		viewModel.HundredthsPart = settings.HundredthsPart;
		viewModel.ThousandthsPart = settings.ThousandthsPart;
		viewModel.UseSmartClick = settings.UseSmartClick;
		viewModel.FreezePointer = settings.FreezePointer;
		viewModel.KeepOnTop = settings.KeepOnTop;

		viewModel.StartStopModifiers = ParseModifierKeys(settings.StartStopModifiers);
		if (Enum.TryParse<Key>(settings.StartStopKey, out var startStopKey))
			viewModel.StartStopKey = startStopKey;

		viewModel.RecordModifiers = ParseModifierKeys(settings.RecordModifiers);
		if (Enum.TryParse<Key>(settings.RecordKey, out var recordKey))
			viewModel.RecordKey = recordKey;

		foreach (var point in settings.Points)
		{
			if (!Enum.TryParse<Core.Models.TargetType>(point.TargetType, out var targetType))
				continue;

			nint? windowId = nint.TryParse(point.TargetWindowId, out var parsed) ? parsed : null;
			viewModel.PointTargets.Add(new PointTargetViewModel(
				targetType,
				point.X,
				point.Y,
				windowId,
				_ => { }));
		}
	}

	private static AppSettings ExtractSettingsFromViewModel(MainViewModel viewModel)
	{
		return new AppSettings
		{
			MouseButton = viewModel.SelectedMouseButton.ToString(),
			PressType = viewModel.SelectedPressType.ToString(),
			ClickMode = viewModel.SelectedClickMode.ToString(),
			DeliveryMode = viewModel.SelectedDeliveryMode.ToString(),
			UseCounter = viewModel.UseCounter,
			MaxClicks = viewModel.MaxClicks,
			HoursPart = viewModel.HoursPart,
			MinutesPart = viewModel.MinutesPart,
			SecondsPart = viewModel.SecondsPart,
			TenthsPart = viewModel.TenthsPart,
			HundredthsPart = viewModel.HundredthsPart,
			ThousandthsPart = viewModel.ThousandthsPart,
			UseSmartClick = viewModel.UseSmartClick,
			FreezePointer = viewModel.FreezePointer,
			KeepOnTop = viewModel.KeepOnTop,
			StartStopModifiers = FormatModifierKeys(viewModel.StartStopModifiers),
			StartStopKey = viewModel.StartStopKey.ToString(),
			RecordModifiers = FormatModifierKeys(viewModel.RecordModifiers),
			RecordKey = viewModel.RecordKey.ToString(),
			Points = viewModel.PointTargets
				.Select(p => new AppPointTarget
				{
					TargetType = p.TargetType.ToString(),
					X = p.X,
					Y = p.Y,
					TargetWindowId = p.WindowId?.ToString(),
				})
				.ToList(),
		};
	}

	private static ModifierKeys ParseModifierKeys(string value)
	{
		var result = ModifierKeys.None;
		foreach (var part in value.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
		{
			if (Enum.TryParse<ModifierKeys>(part, out var modifier))
				result |= modifier;
		}
		return result;
	}

	private static string FormatModifierKeys(ModifierKeys modifiers)
	{
		if (modifiers == ModifierKeys.None)
			return "None";

		var parts = new List<string>();
		foreach (ModifierKeys flag in Enum.GetValues<ModifierKeys>())
		{
			if (flag != ModifierKeys.None && modifiers.HasFlag(flag))
				parts.Add(flag.ToString());
		}
		return string.Join("+", parts);
	}
}

