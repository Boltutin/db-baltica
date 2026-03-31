using System.Data;
using Npgsql;

namespace BaltikaApp.Data
{
    /// <summary>
    /// Доступ к данным PostgreSQL через Npgsql: выборки, изменение данных в режиме записи, пересоздание пулов после смены конфигурации.
    /// </summary>
    public static class Db
    {
        private static readonly object Sync = new();
        private static NpgsqlDataSource? _readerDataSource;
        private static NpgsqlDataSource? _writerDataSource;

        private static NpgsqlDataSource ReaderDataSource
        {
            get
            {
                if (_readerDataSource != null)
                    return _readerDataSource;
                lock (Sync)
                    return _readerDataSource ??= new NpgsqlDataSourceBuilder(DatabaseConnectionSettings.ReaderConnectionString).Build();
            }
        }

        private static NpgsqlDataSource WriterDataSource
        {
            get
            {
                if (_writerDataSource != null)
                    return _writerDataSource;
                lock (Sync)
                    return _writerDataSource ??= new NpgsqlDataSourceBuilder(DatabaseConnectionSettings.WriterConnectionString).Build();
            }
        }

        private static NpgsqlDataSource ActiveDataSource =>
            ConnectionManager.IsWriter ? WriterDataSource : ReaderDataSource;

        /// <summary>
        /// Освобождает текущие источники данных Npgsql и вынуждает пересоздание при следующем обращении (после смены параметров сервера).
        /// </summary>
        public static void ReloadDataSources()
        {
            lock (Sync)
            {
                _readerDataSource?.Dispose();
                _writerDataSource?.Dispose();
                _readerDataSource = null;
                _writerDataSource = null;
            }
        }

        /// <summary>
        /// Выполняет SQL-запрос на чтение и возвращает результат в виде <see cref="DataTable"/>.
        /// Подключение выбирается автоматически в зависимости от текущего режима доступа.
        /// </summary>
        public static DataTable Query(string sql, params NpgsqlParameter[] parameters)
        {
            var dt = new DataTable();
            try
            {
                using var conn = ActiveDataSource.CreateConnection();
                using var cmd = new NpgsqlCommand(sql, conn);
                if (parameters != null && parameters.Length > 0)
                    cmd.Parameters.AddRange(parameters);

                conn.Open();

                using (var da = new NpgsqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            catch (PostgresException ex)
            {
                throw new InvalidOperationException(FormatPostgresError(ex.Message, ex.SqlState), ex);
            }
            catch (NpgsqlException ex)
            {
                throw new InvalidOperationException("Не удалось подключиться к БД. Попробуйте повторить попытку.", ex);
            }
            return dt;
        }

        /// <summary>
        /// Выполняет SQL-команду на изменение данных (INSERT/UPDATE/DELETE).
        /// Доступен только в режиме редактирования; в противном случае выбрасывает исключение.
        /// </summary>
        public static int Execute(string sql, params NpgsqlParameter[] parameters)
        {
            if (!ConnectionManager.IsWriter)
                throw new InvalidOperationException("Операция доступна только в режиме редактирования.");

            using var conn = WriterDataSource.CreateConnection();
            using var cmd = new NpgsqlCommand(sql, conn);
            if (parameters != null && parameters.Length > 0)
                cmd.Parameters.AddRange(parameters);

            try
            {
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (PostgresException ex)
            {
                throw new InvalidOperationException(FormatPostgresError(ex.Message, ex.SqlState), ex);
            }
        }

        private static string FormatPostgresError(string message, string sqlState)
        {
            if (sqlState == "23503")
                return "Нарушение внешнего ключа.";

            if (sqlState == "23505")
                return "Нарушение уникальности (запись уже существует).";

            return "Ошибка БД: " + message;
        }
    }
}
