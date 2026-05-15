using System.Windows;

namespace RjClicker.App.Infrastructure.Points;

public interface IPointCaptureService
{
    Task<Point?> CapturePointAsync(CancellationToken cancellationToken);
}
