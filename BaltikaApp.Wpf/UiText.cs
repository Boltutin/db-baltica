namespace BaltikaApp.Wpf;

/// <summary>
/// Централизованный каталог пользовательских текстов интерфейса.
/// Нужен для единообразия формулировок и упрощения дальнейшей правки.
/// </summary>
internal static class UiText
{
    internal static class Common
    {
        public const string OperationOnlyInWriter = "Операция доступна только в режиме редактирования.";
        public const string SelectShip = "Выберите судно.";
        public const string SelectShipment = "Выберите рейс в таблице.";
        public const string SelectCargo = "Выберите груз в таблице.";
        public const string SelectSender = "Выберите отправителя.";
        public const string SelectConsignee = "Выберите получателя.";
        public const string CrudOnlyForTabs = "CRUD доступен только для вкладок Отправители/Получатели.";
        public const string WrongPort = "Порт должен быть числом от 1 до 65535.";
    }

    internal static class Main
    {
        public const string WriterEnabled = "Режим редактирования включён.";
        public const string WriterPasswordInvalid = "Неверный пароль. Остаётся режим чтения.";
        public const string ReaderEnabled = "Режим чтения включён.";
        public const string ConnectionApplied = "Параметры подключения применены.";
        public const string KpiUpdatedPrefix = "Последнее обновление: ";
        public const string KpiError = "Последнее обновление: ошибка";
        public const string DbAvailable = "Подключение доступно";
        public const string DbSchemaMismatch = "Ошибка: несоответствие структуры БД (обновите SQL-скрипты представлений).";
        public const string DbKpiGenericError = "Ошибка: не удалось обновить показатели. Проверьте подключение и повторите.";
    }

    internal static class Status
    {
        public const string Ready = "Готово к работе.";
        public const string LoadShips = "Загрузка списка судов...";
        public const string LoadShipments = "Загрузка списка рейсов...";
        public const string LoadCargo = "Загрузка списка грузов...";
        public const string LoadClients = "Загрузка данных клиентов...";
        public const string LoadReportPrefix = "Загрузка отчёта «";
        public const string SearchPeriod = "Поиск рейсов за выбранный период...";
    }
}
