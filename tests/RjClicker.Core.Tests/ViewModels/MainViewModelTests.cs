using FluentAssertions;
using RjClicker.App.Core.Models;
using RjClicker.App.Core.Sessions;
using RjClicker.App.Infrastructure.Hotkeys;
using RjClicker.App.Infrastructure.Points;
using RjClicker.App.Infrastructure.Windows;
using RjClicker.App.ViewModels;
using System.Windows;
using RjClicker.Core.Tests.Sessions;

namespace RjClicker.Core.Tests.ViewModels;

public sealed class MainViewModelTests
{
    [Fact]
    public void TotalIntervalMs_ShouldUpdate_WhenIntervalPartsChange()
    {
        var viewModel = CreateViewModel();

        viewModel.HoursPart = 0;
        viewModel.MinutesPart = 0;
        viewModel.SecondsPart = 1;
        viewModel.TenthsPart = 2;
        viewModel.HundredthsPart = 3;
        viewModel.ThousandthsPart = 4;

        viewModel.TotalIntervalMs.Should().Be(1234);
    }

    [Fact]
    public void IntervalParts_ShouldUpdate_WhenTotalIntervalMsChanges()
    {
        var viewModel = CreateViewModel();

        viewModel.TotalIntervalMs = 3723405;

        viewModel.HoursPart.Should().Be(1);
        viewModel.MinutesPart.Should().Be(2);
        viewModel.SecondsPart.Should().Be(3);
        viewModel.TenthsPart.Should().Be(4);
        viewModel.HundredthsPart.Should().Be(0);
        viewModel.ThousandthsPart.Should().Be(5);
    }

    [Fact]
    public void TotalIntervalMs_ShouldClampToZero_WhenSetNegative()
    {
        var viewModel = CreateViewModel();

        viewModel.TotalIntervalMs = -25;

        viewModel.TotalIntervalMs.Should().Be(0);
        viewModel.HoursPart.Should().Be(0);
        viewModel.MinutesPart.Should().Be(0);
        viewModel.SecondsPart.Should().Be(0);
        viewModel.TenthsPart.Should().Be(0);
        viewModel.HundredthsPart.Should().Be(0);
        viewModel.ThousandthsPart.Should().Be(0);
    }

    [Fact]
    public void IntervalParts_ShouldClampToZero_WhenNegativeValueEntered()
    {
        var viewModel = CreateViewModel();

        viewModel.HoursPart = -1;

        viewModel.HoursPart.Should().Be(0);
        viewModel.TotalIntervalMs.Should().Be(100);
    }

    [Fact]
    public void ShouldRaisePropertyChanged_ForTotalIntervalMs_WhenSecondsPartChanges()
    {
        var viewModel = CreateViewModel();
        var changedProperties = new List<string>();

        viewModel.PropertyChanged += (_, args) =>
        {
            if (!string.IsNullOrWhiteSpace(args.PropertyName))
            {
                changedProperties.Add(args.PropertyName);
            }
        };

        viewModel.SecondsPart = 2;

        changedProperties.Should().Contain(nameof(MainViewModel.SecondsPart));
        changedProperties.Should().Contain(nameof(MainViewModel.TotalIntervalMs));
    }

    [Fact]
    public void AddPointCommand_ShouldAddTarget()
    {
        var viewModel = CreateViewModel();

        viewModel.AddPoint.Execute(null);

        viewModel.PointTargets.Should().HaveCount(1);
    }

    [Fact]
    public void ClearPointsCommand_ShouldClearTargets()
    {
        var viewModel = CreateViewModel();
        viewModel.AddPoint.Execute(null);
        viewModel.AddPoint.Execute(null);

        viewModel.ClearPoints.Execute(null);

        viewModel.PointTargets.Should().BeEmpty();
    }

    [Fact]
    public async Task RecordPointCommand_ShouldAppendCapturedPoint()
    {
        var pointCaptureService = new FakePointCaptureService(new Point(100, 200));
        var viewModel = CreateViewModel(pointCaptureService: pointCaptureService);

        viewModel.RecordPoint.Execute(null);
        await Task.Delay(25);

        viewModel.PointTargets.Should().ContainSingle();
        viewModel.PointTargets[0].TargetType.Should().Be(TargetType.Absolute);
        viewModel.PointTargets[0].X.Should().Be(100);
        viewModel.PointTargets[0].Y.Should().Be(200);
    }

    [Fact]
    public async Task StartAndStopCommands_ShouldDriveRunningState()
    {
        var dispatcher = new FakeDispatcher();
        var scheduler = new SlowFakeScheduler();
        var hotkeyService = new FakeHotkeyService();
        var controller = new ClickSessionController(dispatcher, scheduler, hotkeyService);
        var viewModel = CreateViewModel(controller: controller);

        viewModel.AddPoint.Execute(null);
        viewModel.StartClickSession.Execute(null);
        await Task.Delay(30);

        viewModel.IsRunning.Should().BeTrue();
        viewModel.StopClickSession.Execute(null);
        await Task.Delay(30);

        viewModel.IsRunning.Should().BeFalse();
    }

    [Fact]
    public void RemovePointCommand_ShouldRemoveSpecifiedTarget()
    {
        var viewModel = CreateViewModel();
        viewModel.AddPoint.Execute(null);
        var target = viewModel.PointTargets[0];

        viewModel.RemovePoint.Execute(target);

        viewModel.PointTargets.Should().BeEmpty();
    }

    private static MainViewModel CreateViewModel(
        ClickSessionController? controller = null,
        IPointCaptureService? pointCaptureService = null,
        IWindowBindingService? windowBindingService = null)
    {
        controller ??= new ClickSessionController(new FakeDispatcher(), new FakeScheduler(maxTicks: 1), new FakeHotkeyService());
        pointCaptureService ??= new FakePointCaptureService(null);
        windowBindingService ??= new FakeWindowBindingService();

        return new MainViewModel(controller, pointCaptureService, windowBindingService);
    }

    private sealed class FakePointCaptureService : IPointCaptureService
    {
        private readonly Point? _point;

        public FakePointCaptureService(Point? point)
        {
            _point = point;
        }

        public Task<Point?> CapturePointAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_point);
        }
    }

    private sealed class FakeWindowBindingService : IWindowBindingService
    {
        public Task<nint> GetWindowHandleAsync(string windowTitle)
        {
            return Task.FromResult(nint.Zero);
        }

        public Task<Rect> GetWindowBoundsAsync(nint windowHandle)
        {
            return Task.FromResult(Rect.Empty);
        }
    }
}
