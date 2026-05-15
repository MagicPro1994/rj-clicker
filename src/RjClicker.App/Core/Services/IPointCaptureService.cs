using System.Windows;

namespace RjClicker.App.Core.Services;

public interface IPointCaptureService
{
    Task<Point?> CapturePointAsync(CancellationToken cancellationToken);
}
