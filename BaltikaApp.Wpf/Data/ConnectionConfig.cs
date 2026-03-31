namespace BaltikaApp.Data
{
    /// <summary>
    /// Сериализуемые параметры экземпляра PostgreSQL: хост, порт, имя базы данных.
    /// </summary>
    internal sealed class ConnectionConfig
    {
        public string Host { get; set; } = "localhost";

        public int Port { get; set; } = 5432;

        public string Database { get; set; } = "baltika";
    }
}
