using System;
using System.IO;
using System.Text.Json;

namespace BaltikaApp.Data
{
    /// <summary>
    /// Чтение и запись файла пользовательских параметров подключения в каталоге локальных данных приложения.
    /// </summary>
    internal static class ConnectionConfigStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private static string ConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "BaltikaApp", "connection.json");

        /// <summary>
        /// Загружает ранее сохранённые параметры подключения.
        /// Возвращает <c>false</c>, если файл отсутствует, повреждён или содержит некорректные значения.
        /// </summary>
        public static bool TryLoad(out ConnectionConfig config)
        {
            config = new ConnectionConfig();
            try
            {
                if (!File.Exists(ConfigPath))
                    return false;

                var json = File.ReadAllText(ConfigPath);
                var loaded = JsonSerializer.Deserialize<ConnectionConfig>(json, JsonOptions);
                if (loaded == null || string.IsNullOrWhiteSpace(loaded.Host) || string.IsNullOrWhiteSpace(loaded.Database))
                    return false;
                if (loaded.Port < 1 || loaded.Port > 65535)
                    return false;

                config = loaded;
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Сериализует параметры подключения в JSON-файл. Каталог создаётся при необходимости.
        /// </summary>
        public static void Save(ConnectionConfig config)
        {
            var dir = Path.GetDirectoryName(ConfigPath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(ConfigPath, JsonSerializer.Serialize(config, JsonOptions));
        }
    }
}
