using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Точка запуска WPF-приложения: инициализация настроек подключения,
/// проверка доступности PostgreSQL и создание главного окна.
/// </summary>
public partial class App : System.Windows.Application
{
    /// <summary>Обработчик события <c>Startup</c>: инициализация конфигурации, проверка БД, создание главного окна.</summary>
    private void OnStartup(object sender, StartupEventArgs e)
    {
        ThemeManager.Initialize();
        DatabaseConnectionSettings.Initialize();
        ConnectionManager.UseReader();

        if (!TryStartupDatabaseCheck())
        {
            Shutdown(-1);
            return;
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    /// <summary>Выполняет пробное подключение к PostgreSQL. При неудаче показывает сообщение и возвращает <c>false</c>.</summary>
    private static bool TryStartupDatabaseCheck()
    {
        try
        {
            using var conn = new NpgsqlConnection(
                DatabaseConnectionSettings.BuildConnectionString("baltika_reader", "123", 3));
            conn.Open();
            return true;
        }
        catch
        {
            WpfMessageService.ShowError(
                "Не удалось подключиться к PostgreSQL.\r\n\r\n" +
                "Проверьте запуск сервера, хост/порт и наличие базы данных.");
            return false;
        }
    }
}

