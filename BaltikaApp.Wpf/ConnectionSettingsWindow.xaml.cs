using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно настройки параметров подключения к PostgreSQL (хост, порт, база данных).
/// Поддерживает проверку соединения, запуск локального PostgreSQL
/// (как Windows-служба или portable-экземпляр) и сохранение настроек.
/// </summary>
public partial class ConnectionSettingsWindow : Window
{
    public ConnectionSettingsWindow()
    {
        InitializeComponent();
        HostBox.Text = DatabaseConnectionSettings.Host;
        PortBox.Text = DatabaseConnectionSettings.Port.ToString();
        DbBox.Text = DatabaseConnectionSettings.Database;
        DarkThemeCheck.IsChecked = ThemeManager.CurrentMode == AppThemeMode.Dark;
        UpdateStatus();
    }

    private void OnTest(object sender, RoutedEventArgs e)
    {
        if (!TryGetValidatedInput(out var host, out var port, out var db))
            return;
        try
        {
            ActionBarTop.IsEnabled = false;
            ActionBarBottom.IsEnabled = false;
            StatusUiHelper.SetStatus(OperationStatusText, "Проверка подключения к серверу...");
            var cs = new NpgsqlConnectionStringBuilder
            {
                Host = host,
                Port = port,
                Database = db,
                Username = "baltika_reader",
                Password = "123",
                Timeout = 5
            }.ConnectionString;
            using var conn = new NpgsqlConnection(cs);
            conn.Open();
            StatusUiHelper.SetStatus(OperationStatusText, "Подключение успешно. Можно сохранить параметры.", StatusKind.Success);
            WpfMessageService.ShowInfo("Подключение к серверу успешно.");
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(OperationStatusText, "Подключение не удалось. Проверьте параметры сервера.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Проверка подключения", ex);
        }
        finally
        {
            ActionBarTop.IsEnabled = true;
            ActionBarBottom.IsEnabled = true;
            UpdateStatus();
        }
    }

    private void OnStartLocal(object sender, RoutedEventArgs e)
    {
        try
        {
            ActionBarTop.IsEnabled = false;
            ActionBarBottom.IsEnabled = false;
            StatusUiHelper.SetStatus(OperationStatusText, "Запуск локального PostgreSQL...");
            if (TryStartLocalService(out var name))
            {
                StatusUiHelper.SetStatus(OperationStatusText, $"Локальный PostgreSQL запущен: {name}.", StatusKind.Success);
                WpfMessageService.ShowInfo($"Локальный PostgreSQL запущен ({name}).");
                UpdateStatus();
                return;
            }

            if (TryStartPortable(out var msg))
            {
                StatusUiHelper.SetStatus(OperationStatusText, msg, StatusKind.Success);
                WpfMessageService.ShowInfo(msg);
                UpdateStatus();
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = AppContext.BaseDirectory,
                UseShellExecute = true
            });
            WpfMessageService.ShowInfo("Служба/портативный PostgreSQL не найдены. Открыт каталог приложения.");
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(OperationStatusText, "Не удалось запустить локальный сервер PostgreSQL.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Запуск локального PostgreSQL", ex);
        }
        finally
        {
            ActionBarTop.IsEnabled = true;
            ActionBarBottom.IsEnabled = true;
            UpdateStatus();
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (!TryGetValidatedInput(out var host, out var port, out var db))
            return;
        DatabaseConnectionSettings.ApplyAndSave(host, port, db);
        Db.ReloadDataSources();
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void OnThemeChanged(object sender, RoutedEventArgs e)
    {
        var mode = DarkThemeCheck.IsChecked == true ? AppThemeMode.Dark : AppThemeMode.Light;
        ThemeManager.Apply(mode);
        ThemeManager.Save(mode);
    }

    private bool TryGetValidatedInput(out string host, out int port, out string db)
    {
        host = HostBox.Text.Trim();
        db = DbBox.Text.Trim();
        port = 0;
        if (!ValidationService.ValidateConnectionInput(host, db, out var err))
        {
            WpfMessageService.ShowError(err);
            return false;
        }
        if (!int.TryParse(PortBox.Text.Trim(), out port) || port < 1 || port > 65535)
        {
            WpfMessageService.ShowError(UiText.Common.WrongPort);
            return false;
        }
        return true;
    }

    private void UpdateStatus()
    {
        var serviceName = FindPostgresServiceName();
        if (!string.IsNullOrEmpty(serviceName))
        {
            StatusText.Text = $"Статус локального PostgreSQL: {(IsServiceRunning(serviceName) ? "работает" : "остановлен")} ({serviceName})";
            return;
        }

        StatusText.Text = GetPortableStatus();
    }

    /// <summary>Ищет Windows-службу PostgreSQL и пытается её запустить. Ожидает до 12 секунд.</summary>
    private static bool TryStartLocalService(out string serviceName)
    {
        serviceName = FindPostgresServiceName();
        if (string.IsNullOrEmpty(serviceName))
            return false;
        if (IsServiceRunning(serviceName))
            return true;
        ExecuteShell("sc", $"start \"{serviceName}\"");
        var until = DateTime.UtcNow.AddSeconds(12);
        while (DateTime.UtcNow < until)
        {
            if (IsServiceRunning(serviceName))
                return true;
            System.Threading.Thread.Sleep(500);
        }
        return false;
    }

    /// <summary>Перечисляет системные службы и возвращает имя первой, содержащей «postgres».</summary>
    private static string FindPostgresServiceName()
    {
        var output = ExecuteShell("sc", "query type= service state= all");
        var lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (!line.StartsWith("SERVICE_NAME:", StringComparison.OrdinalIgnoreCase))
                continue;
            var name = line.Substring("SERVICE_NAME:".Length).Trim();
            if (name.Contains("postgres", StringComparison.OrdinalIgnoreCase))
                return name;
        }
        return string.Empty;
    }

    private static bool IsServiceRunning(string serviceName)
        => ExecuteShell("sc", $"query \"{serviceName}\"").Contains("RUNNING", StringComparison.OrdinalIgnoreCase);

    /// <summary>Ищет portable-установку PostgreSQL рядом с исполняемым файлом и запускает pg_ctl start.</summary>
    private static bool TryStartPortable(out string message)
    {
        message = string.Empty;
        if (!TryFindPortable(out var pgCtl, out var dataDir))
            return false;
        var output = ExecuteShell(pgCtl, $"start -D \"{dataDir}\" -l \"{Path.Combine(dataDir, "pg-start.log")}\" -w -t 12");
        if (output.Contains("server started", StringComparison.OrdinalIgnoreCase) || output.Contains("already running", StringComparison.OrdinalIgnoreCase))
        {
            message = $"Портативный PostgreSQL запущен ({dataDir}).";
            return true;
        }
        return false;
    }

    private static string GetPortableStatus()
    {
        if (!TryFindPortable(out var pgCtl, out var dataDir))
            return "Статус локального PostgreSQL: служба не найдена, портативный сервер не обнаружен";
        var output = ExecuteShell(pgCtl, $"status -D \"{dataDir}\"");
        return output.Contains("server is running", StringComparison.OrdinalIgnoreCase)
            ? $"Статус локального PostgreSQL: работает (portable, {dataDir})"
            : $"Статус локального PostgreSQL: остановлен (portable, {dataDir})";
    }

    /// <summary>Проверяет наличие каталогов <c>postgres/</c>, <c>pgsql/</c> или <c>pg/</c> рядом с приложением.</summary>
    private static bool TryFindPortable(out string pgCtlPath, out string dataDir)
    {
        pgCtlPath = string.Empty;
        dataDir = string.Empty;
        var baseDir = AppContext.BaseDirectory;
        var roots = new[] { Path.Combine(baseDir, "postgres"), Path.Combine(baseDir, "pgsql"), Path.Combine(baseDir, "pg") };
        foreach (var root in roots)
        {
            var ctl = Path.Combine(root, "bin", "pg_ctl.exe");
            var data = Path.Combine(root, "data");
            if (File.Exists(ctl) && Directory.Exists(data))
            {
                pgCtlPath = ctl;
                dataDir = data;
                return true;
            }
        }
        return false;
    }

    /// <summary>Запускает внешний процесс и возвращает объединённый вывод stdout и stderr.</summary>
    private static string ExecuteShell(string fileName, string args)
    {
        using var p = new Process();
        p.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        p.Start();
        var std = p.StandardOutput.ReadToEnd();
        var err = p.StandardError.ReadToEnd();
        p.WaitForExit();
        return std + Environment.NewLine + err;
    }

}
