namespace BaltikaApp.Wpf;

/// <summary>
/// Вспомогательные методы проверки пользовательского ввода для WPF-форм.
/// </summary>
internal static class ValidationService
{
    /// <summary>
    /// Проверяет корректность параметров подключения к БД.
    /// Возвращает <c>false</c> и заполняет <paramref name="error"/>, если данные невалидны.
    /// </summary>
    public static bool ValidateConnectionInput(string host, string database, out string error)
    {
        error = string.Empty;
        if (string.IsNullOrWhiteSpace(host))
        {
            error = "Не указан хост подключения.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(database))
        {
            error = "Не указано имя базы данных.";
            return false;
        }

        return true;
    }
}
