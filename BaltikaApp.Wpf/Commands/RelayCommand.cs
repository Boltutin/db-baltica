using System;
using System.Windows.Input;

namespace BaltikaApp.Wpf.Commands;

/// <summary>
/// Универсальная реализация <see cref="ICommand"/> на делегатах.
/// Поддерживает как синхронное, так и параметризованное выполнение.
/// </summary>
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    /// <inheritdoc/>
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    /// <param name="execute">Действие, выполняемое командой.</param>
    /// <param name="canExecute">Предикат доступности (необязателен).</param>
    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    /// <summary>Удобный конструктор без параметра команды.</summary>
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
        : this(_ => execute(), canExecute == null ? null : _ => canExecute()) { }

    /// <inheritdoc/>
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    /// <inheritdoc/>
    public void Execute(object? parameter) => _execute(parameter);

    /// <summary>Принудительно уведомляет WPF о пересчёте доступности команды.</summary>
    public static void Refresh() => CommandManager.InvalidateRequerySuggested();
}
