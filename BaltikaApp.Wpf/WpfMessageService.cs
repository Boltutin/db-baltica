using System;

namespace BaltikaApp.Wpf;

/// <summary>
/// Обёртка над <see cref="System.Windows.MessageBox"/> для унифицированного вывода
/// информационных сообщений, ошибок и запросов подтверждения в WPF-интерфейсе.
/// </summary>
internal static class WpfMessageService
{
    /// <summary>Отображает информационное сообщение.</summary>
    public static void ShowInfo(string message, string title = "Информация")
        => System.Windows.MessageBox.Show(
            message, title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Information);

    /// <summary>Отображает сообщение об ошибке.</summary>
    public static void ShowError(string message, string title = "Ошибка")
        => System.Windows.MessageBox.Show(
            message, title,
            System.Windows.MessageBoxButton.OK,
            System.Windows.MessageBoxImage.Error);

    /// <summary>Запрашивает подтверждение действия (Да/Нет). Возвращает <c>true</c>, если пользователь нажал «Да».</summary>
    public static bool Confirm(string message, string title = "Подтверждение")
        => System.Windows.MessageBox.Show(
            message, title,
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question) == System.Windows.MessageBoxResult.Yes;

    /// <summary>
    /// Показывает пользователю понятное сообщение об ошибке с короткой рекомендацией.
    /// </summary>
    public static void ShowOperationError(string operation, Exception ex)
    {
        var details = ex.Message;
        var recommendation = "Проверьте подключение к БД и повторите действие.";

        if (details.Contains("42703"))
            recommendation = "Структура БД не совпадает с приложением. Обновите SQL-скрипты представлений.";
        else if (details.Contains("23503"))
            recommendation = "Связанная запись используется в других данных. Сначала удалите зависимые записи.";
        else if (details.Contains("23505"))
            recommendation = "Такая запись уже существует. Проверьте уникальные поля.";
        else if (details.Contains("подключиться к БД", StringComparison.OrdinalIgnoreCase))
            recommendation = "Проверьте хост, порт, имя базы и доступность сервера PostgreSQL.";

        ShowError($"{operation} не выполнено.\r\n\r\n{recommendation}\r\n\r\nТехнические детали: {details}");
    }
}
