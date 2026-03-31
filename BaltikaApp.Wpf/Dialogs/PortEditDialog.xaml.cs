using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления/редактирования порта.</summary>
public partial class PortEditDialog : Window
{
    private readonly int? _portId;

    public PortEditDialog(int? portId = null)
    {
        _portId = portId;
        InitializeComponent();
        Title = portId.HasValue ? "Изменить порт" : "Добавить порт";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadAddresses();

            if (!_portId.HasValue) return;
            var dt = Db.Query("SELECT port_name, address_id FROM ports WHERE port_id=@id;",
                new NpgsqlParameter("@id", _portId.Value));
            if (dt.Rows.Count == 0) throw new InvalidOperationException("Порт не найден.");

            PortNameBox.Text = Convert.ToString(dt.Rows[0]["port_name"]) ?? string.Empty;
            AddressBox.SelectedValue = Convert.ToInt32(dt.Rows[0]["address_id"]);
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Загрузка порта", ex);
            DialogResult = false;
        }
    }

    private void LoadAddresses()
    {
        AddressBox.ItemsSource = Db.Query(
            @"SELECT address_id,
                     (country || ', ' || COALESCE(region || ', ', '') || city || ', ' || street || ' ' || building_number) AS caption
              FROM addresses
              ORDER BY country, city, street, building_number;").DefaultView;
    }

    private void OnAddAddress(object sender, RoutedEventArgs e)
    {
        if (!ConnectionManager.IsWriter)
        {
            WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter);
            return;
        }

        var dlg = new AddressEditDialog { Owner = this };
        if (dlg.ShowDialog() == true)
            LoadAddresses();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var portName = PortNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(portName))
        {
            WpfMessageService.ShowInfo("Введите название порта.", "Проверка");
            return;
        }
        if (AddressBox.SelectedValue == null)
        {
            WpfMessageService.ShowInfo("Выберите адрес порта.", "Проверка");
            return;
        }

        try
        {
            if (_portId.HasValue)
            {
                Db.Execute("UPDATE ports SET port_name=@name, address_id=@address, updated_at=NOW() WHERE port_id=@id;",
                    new NpgsqlParameter("@name", portName),
                    new NpgsqlParameter("@address", Convert.ToInt32(AddressBox.SelectedValue)),
                    new NpgsqlParameter("@id", _portId.Value));
            }
            else
            {
                Db.Execute("INSERT INTO ports (port_name, address_id) VALUES (@name, @address);",
                    new NpgsqlParameter("@name", portName),
                    new NpgsqlParameter("@address", Convert.ToInt32(AddressBox.SelectedValue)));
            }
            DialogResult = true;
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Сохранение порта", ex);
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
