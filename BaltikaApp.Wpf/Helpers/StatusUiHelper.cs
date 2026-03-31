using System.Windows.Controls;
using System.Windows.Media;

namespace BaltikaApp.Wpf.Helpers;

internal enum StatusKind
{
    Info,
    Success,
    Warning,
    Error
}

internal static class StatusUiHelper
{
    public static void SetStatus(TextBlock target, string message, StatusKind kind = StatusKind.Info)
    {
        target.Text = message;
        target.Foreground = kind switch
        {
            StatusKind.Success => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E7D32")),
            StatusKind.Warning => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A6700")),
            StatusKind.Error => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3261E")),
            _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5D6D7E"))
        };
    }
}
