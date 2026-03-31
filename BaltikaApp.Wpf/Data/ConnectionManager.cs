namespace BaltikaApp.Data
{
    /// <summary>
    /// Текущий режим доступа к БД: чтение (baltika_reader) или редактирование (baltika_writer). Строки подключения формируются из <see cref="DatabaseConnectionSettings"/>.
    /// </summary>
    public static class ConnectionManager
    {
        /// <summary>Перечисление допустимых режимов доступа к БД.</summary>
        public enum DbMode { Reader, Writer }

        private static DbMode _mode = DbMode.Reader;

        /// <summary>Возвращает <c>true</c>, если приложение работает в режиме редактирования (роль <c>baltika_writer</c>).</summary>
        public static bool IsWriter => _mode == DbMode.Writer;

        /// <summary>Событие, уведомляющее подписчиков о смене режима доступа.</summary>
        public static event Action? ModeChanged;

        /// <summary>Переключает приложение в режим только чтения (роль <c>baltika_reader</c>).</summary>
        public static void UseReader()
        {
            _mode = DbMode.Reader;
            ModeChanged?.Invoke();
        }

        /// <summary>
        /// Пытается переключить приложение в режим редактирования.
        /// Возвращает <c>true</c> при успешной аутентификации.
        /// </summary>
        public static bool TrySwitchToWriter(string password)
        {
            if (password == "12345")
            {
                _mode = DbMode.Writer;
                ModeChanged?.Invoke();
                return true;
            }
            return false;
        }
    }
}
