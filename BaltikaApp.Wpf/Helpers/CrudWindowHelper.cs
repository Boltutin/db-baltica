using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BaltikaApp.Data;

namespace BaltikaApp.Wpf.Helpers;

/// <summary>
/// Управление CRUD-кнопками в зависимости от текущего режима доступа.
/// Подписывается на <see cref="ConnectionManager.ModeChanged"/> и отписывается при закрытии окна.
/// </summary>
internal sealed class CrudWindowHelper : IDisposable
{
    private readonly Button[] _buttons;
    private readonly Dispatcher _dispatcher;

    public CrudWindowHelper(Window owner, params Button[] crudButtons)
    {
        _buttons = crudButtons;
        _dispatcher = owner.Dispatcher;
        ConnectionManager.ModeChanged += OnModeChanged;
        owner.Closed += (_, _) => Dispose();
        UpdateButtons();
    }

    /// <summary>Синхронизирует доступность CRUD-кнопок с текущим режимом доступа.</summary>
    public void UpdateButtons()
    {
        var enabled = ConnectionManager.IsWriter;
        foreach (var btn in _buttons)
            btn.IsEnabled = enabled;
    }

    /// <summary>
    /// Проверяет, что приложение находится в режиме редактирования.
    /// Если нет — показывает информационное сообщение и возвращает <c>false</c>.
    /// </summary>
    public static bool RequireWriter()
    {
        if (ConnectionManager.IsWriter) return true;
        WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter);
        return false;
    }

    public void Dispose()
    {
        ConnectionManager.ModeChanged -= OnModeChanged;
    }

    private void OnModeChanged() => _dispatcher.Invoke(UpdateButtons);
}
