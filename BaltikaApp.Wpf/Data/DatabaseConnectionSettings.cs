using System;
using Npgsql;

namespace BaltikaApp.Data
{
    /// <summary>
    /// Параметры подключения к PostgreSQL: переменные окружения, локальный файл конфигурации
    /// (%LocalAppData%\BaltikaApp\connection.json), значения по умолчанию.
    /// Переменные: BALTIKA_HOST, BALTIKA_PORT, BALTIKA_DB, BALTIKA_ADMIN_USER, BALTIKA_ADMIN_PASSWORD.
    /// </summary>
    internal static class DatabaseConnectionSettings
    {
        private static string _host = "localhost";
        private static int _port = 5432;
        private static string _database = "baltika";

        public static string Host => _host;
        public static int Port => _port;
        public static string Database => _database;

        public static string AdminUsername { get; } = Environment.GetEnvironmentVariable("BALTIKA_ADMIN_USER") ?? "postgres";

        public static string AdminPassword { get; } = Environment.GetEnvironmentVariable("BALTIKA_ADMIN_PASSWORD") ?? "postgres";

        /// <summary>
        /// Инициализирует параметры из окружения и сохранённого файла. Вызывается один раз при запуске, до обращения к данным.
        /// </summary>
        public static void Initialize()
        {
            _host = Environment.GetEnvironmentVariable("BALTIKA_HOST") ?? "localhost";
            _port = int.TryParse(Environment.GetEnvironmentVariable("BALTIKA_PORT"), out var p) ? p : 5432;
            _database = Environment.GetEnvironmentVariable("BALTIKA_DB") ?? "baltika";

            if (ConnectionConfigStore.TryLoad(out var saved))
            {
                _host = saved.Host.Trim();
                _port = saved.Port;
                _database = saved.Database.Trim();
            }
        }

        /// <summary>
        /// Сохраняет введённые пользователем хост, порт и имя базы в локальный файл. После вызова необходимо выполнить <see cref="Db.ReloadDataSources"/>.
        /// </summary>
        public static void ApplyAndSave(string host, int port, string database)
        {
            _host = host.Trim();
            _port = port;
            _database = database.Trim();
            ConnectionConfigStore.Save(new ConnectionConfig
            {
                Host = _host,
                Port = _port,
                Database = _database
            });
        }

        /// <summary>
        /// Формирует строку подключения Npgsql из текущих параметров хоста/порта/базы и указанных учётных данных.
        /// </summary>
        public static string BuildConnectionString(string username, string password, int? connectionTimeoutSeconds = null)
        {
            var csb = new NpgsqlConnectionStringBuilder
            {
                Host = Host,
                Port = Port,
                Database = Database,
                Username = username,
                Password = password
            };
            if (connectionTimeoutSeconds.HasValue)
                csb.Timeout = connectionTimeoutSeconds.Value;
            return csb.ConnectionString;
        }

        /// <summary>Строка подключения для роли чтения (<c>baltika_reader</c>).</summary>
        public static string ReaderConnectionString => BuildConnectionString("baltika_reader", "123");

        /// <summary>Строка подключения для роли редактирования (<c>baltika_writer</c>).</summary>
        public static string WriterConnectionString => BuildConnectionString("baltika_writer", "12345");

        /// <summary>Строка подключения для административной роли (создание объектов БД, управление).</summary>
        public static string AdminConnectionString => BuildConnectionString(AdminUsername, AdminPassword);
    }
}
