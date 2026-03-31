using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления/редактирования капитана.</summary>
public partial class CaptainEditDialog : Window
{
    private readonly int? _captainId;

    public CaptainEditDialog(int? captainId = null)
    {
        _captainId = captainId;
        InitializeComponent();
        Title = captainId.HasValue ? "Изменить капитана" : "Добавить капитана";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_captainId.HasValue) return;
        try
        {
            var dt = Db.Query("SELECT full_name, experience FROM captains WHERE captain_id=@id;",
                new NpgsqlParameter("@id", _captainId.Value));
            if (dt.Rows.Count == 0) throw new InvalidOperationException("Капитан не найден.");

            NameBox.Text = Convert.ToString(dt.Rows[0]["full_name"]) ?? string.Empty;
            ExperienceBox.Text = Convert.ToString(dt.Rows[0]["experience"]) ?? "0";
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Загрузка капитана", ex);
            DialogResult = false;
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            WpfMessageService.ShowInfo("Введите ФИО капитана.", "Проверка");
            return;
        }

        if (!int.TryParse(ExperienceBox.Text.Trim(), out var experience) || experience < 0)
        {
            WpfMessageService.ShowInfo("Стаж должен быть целым числом не меньше 0.", "Проверка");
            return;
        }

        try
        {
            if (_captainId.HasValue)
            {
                Db.Execute(
                    "UPDATE captains SET full_name=@name, experience=@exp, updated_at=NOW() WHERE captain_id=@id;",
                    new NpgsqlParameter("@name", name),
                    new NpgsqlParameter("@exp", experience),
                    new NpgsqlParameter("@id", _captainId.Value));
            }
            else
            {
                Db.Execute(
                    "INSERT INTO captains (full_name, experience) VALUES (@name, @exp);",
                    new NpgsqlParameter("@name", name),
                    new NpgsqlParameter("@exp", experience));
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Сохранение капитана", ex);
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
