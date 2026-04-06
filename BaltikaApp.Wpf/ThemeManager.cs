using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace BaltikaApp.Wpf;

internal enum AppThemeMode
{
    Light,
    Dark
}

/// <summary>Светлая/тёмная тема: подмена кистей в ресурсах приложения и сохранение выбора в <c>%LocalAppData%\BaltikaApp\ui-theme.json</c>.</summary>
internal static class ThemeManager
{
    private const string ThemeConfigFileName = "ui-theme.json";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static AppThemeMode CurrentMode { get; private set; } = AppThemeMode.Light;

    public static void Initialize()
    {
        var mode = Load();
        Apply(mode);
    }

    public static void Apply(AppThemeMode mode)
    {
        CurrentMode = mode;
        var resources = Application.Current.Resources;

        if (mode == AppThemeMode.Dark)
        {
            SetBrushColor(resources, "PrimaryBrush", "#2F81C1");
            SetBrushColor(resources, "PrimaryHoverBrush", "#3E96DB");
            SetBrushColor(resources, "AccentBrush", "#17A2B8");
            SetBrushColor(resources, "AccentHoverBrush", "#1FBAD3");
            SetBrushColor(resources, "DangerBrush", "#E35D5D");
            SetBrushColor(resources, "DangerHoverBrush", "#F07171");
            SetBrushColor(resources, "NeutralBrush", "#64748B");
            SetBrushColor(resources, "NeutralHoverBrush", "#7A8CA5");
            SetBrushColor(resources, "SurfaceBrush", "#111827");
            SetBrushColor(resources, "BorderBrush", "#374151");
            SetBrushColor(resources, "ForegroundBrush", "#E5E7EB");
            SetBrushColor(resources, "InputBackgroundBrush", "#1F2937");
            SetBrushColor(resources, "InputForegroundBrush", "#F3F4F6");
            SetBrushColor(resources, "GridRowHoverBrush", "#2B3A4A");
            SetBrushColor(resources, "GridRowSelectedBrush", "#35506B");
        }
        else
        {
            SetBrushColor(resources, "PrimaryBrush", "#0F4C81");
            SetBrushColor(resources, "PrimaryHoverBrush", "#115B99");
            SetBrushColor(resources, "AccentBrush", "#006C9C");
            SetBrushColor(resources, "AccentHoverBrush", "#007EBA");
            SetBrushColor(resources, "DangerBrush", "#B9382A");
            SetBrushColor(resources, "DangerHoverBrush", "#D64B3A");
            SetBrushColor(resources, "NeutralBrush", "#475569");
            SetBrushColor(resources, "NeutralHoverBrush", "#5A6D86");
            SetBrushColor(resources, "SurfaceBrush", "#EEF2F6");
            SetBrushColor(resources, "BorderBrush", "#B8C3CF");
            SetBrushColor(resources, "ForegroundBrush", "#0F172A");
            SetBrushColor(resources, "InputBackgroundBrush", "#FFFFFF");
            SetBrushColor(resources, "InputForegroundBrush", "#0F172A");
            SetBrushColor(resources, "GridRowHoverBrush", "#DCE8F5");
            SetBrushColor(resources, "GridRowSelectedBrush", "#BFD7EE");
        }
    }

    public static void Save(AppThemeMode mode)
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "BaltikaApp");
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, ThemeConfigFileName);
        var payload = JsonSerializer.Serialize(new ThemeConfig { Theme = mode.ToString() }, JsonOptions);
        File.WriteAllText(path, payload);
    }

    private static AppThemeMode Load()
    {
        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BaltikaApp",
                ThemeConfigFileName);
            if (!File.Exists(path))
                return AppThemeMode.Light;

            var raw = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<ThemeConfig>(raw, JsonOptions);
            if (config == null || string.IsNullOrWhiteSpace(config.Theme))
                return AppThemeMode.Light;
            return Enum.TryParse<AppThemeMode>(config.Theme, true, out var mode)
                ? mode
                : AppThemeMode.Light;
        }
        catch
        {
            return AppThemeMode.Light;
        }
    }

    private static void SetBrushColor(ResourceDictionary resources, string key, string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        // Some brushes loaded from XAML are frozen (read-only), so replace resource instead of mutating it.
        resources[key] = new SolidColorBrush(color);
    }

    private sealed class ThemeConfig
    {
        public string Theme { get; set; } = "Light";
    }
}
