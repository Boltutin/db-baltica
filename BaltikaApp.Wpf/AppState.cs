using System.ComponentModel;
using System.Runtime.CompilerServices;
using BaltikaApp.Data;

namespace BaltikaApp.Wpf;

/// <summary>
/// Состояние главного окна: режим доступа к БД, краткое описание сервера (<see cref="ConnectionSummary"/>), текст статуса.
/// Реализует <see cref="INotifyPropertyChanged"/> для привязки XAML.
/// Подписывается на <see cref="ConnectionManager.ModeChanged"/> и отписывается в <see cref="Dispose"/>.
/// </summary>
public sealed class AppState : INotifyPropertyChanged, IDisposable
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _dbStatus = "Не проверено";

    /// <summary>Текстовое описание текущего режима доступа («Редактирование» / «Только чтение»).</summary>
    public string AccessMode => ConnectionManager.IsWriter ? "Редактирование" : "Только чтение";

    /// <summary>Имя роли PostgreSQL, используемой в текущем сеансе.</summary>
    public string AccessRole => ConnectionManager.IsWriter ? "baltika_writer" : "baltika_reader";

    /// <summary>Краткая строка вида «host:port / db» для отображения в строке статуса.</summary>
    public string ConnectionSummary =>
        $"{DatabaseConnectionSettings.Host}:{DatabaseConnectionSettings.Port} / {DatabaseConnectionSettings.Database}";

    /// <summary>Статус последней проверки подключения к БД.</summary>
    public string DbStatus
    {
        get => _dbStatus;
        set
        {
            _dbStatus = value;
            OnPropertyChanged();
        }
    }

    public AppState()
    {
        ConnectionManager.ModeChanged += OnModeChanged;
    }

    /// <summary>Принудительно обновляет все свойства, зависящие от настроек подключения и режима доступа.</summary>
    public void RefreshConnectionState()
    {
        OnPropertyChanged(nameof(ConnectionSummary));
        OnPropertyChanged(nameof(AccessMode));
        OnPropertyChanged(nameof(AccessRole));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ConnectionManager.ModeChanged -= OnModeChanged;
    }

    private void OnModeChanged() => RefreshConnectionState();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
