using System;
using System.Windows;
using System.Windows.Controls;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Dialogs;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно ведения ключевых справочников (вариант C):
/// капитаны, порты и адреса.
/// </summary>
public partial class ReferenceDataWindow : Window
{
    private readonly CrudWindowHelper _crudCaptains;
    private readonly CrudWindowHelper _crudPorts;
    private readonly CrudWindowHelper _crudAddresses;

    public ReferenceDataWindow()
    {
        InitializeComponent();
        _crudCaptains = new CrudWindowHelper(this, AddCaptainButton, EditCaptainButton, DeleteCaptainButton);
        _crudPorts = new CrudWindowHelper(this, AddPortButton, EditPortButton, DeletePortButton);
        _crudAddresses = new CrudWindowHelper(this, AddAddressButton, EditAddressButton, DeleteAddressButton);

        LoadCaptains();
        LoadPorts();
        LoadAddresses();
    }

    private void LoadCaptains()
    {
        try
        {
            CaptainsGrid.ItemsSource = Db.Query(
                "SELECT captain_id, full_name AS \"ФИО\", experience AS \"Стаж (лет)\" FROM captains ORDER BY full_name;")
                .DefaultView;
            StatusUiHelper.SetStatus(StatusText, "Справочник капитанов загружен.", StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить капитанов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка капитанов", ex);
        }
    }

    private void LoadPorts()
    {
        try
        {
            PortsGrid.ItemsSource = Db.Query(
                @"SELECT p.port_id,
                         p.port_name AS ""Порт"",
                         (a.country || ', ' || COALESCE(a.region || ', ', '') || a.city || ', ' || a.street || ' ' || a.building_number) AS ""Адрес""
                  FROM ports p
                  LEFT JOIN addresses a ON p.address_id = a.address_id
                  ORDER BY p.port_name;").DefaultView;
            StatusUiHelper.SetStatus(StatusText, "Справочник портов загружен.", StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить порты.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка портов", ex);
        }
    }

    private void LoadAddresses()
    {
        try
        {
            AddressesGrid.ItemsSource = Db.Query(
                @"SELECT address_id,
                         country AS ""Страна"",
                         region AS ""Регион"",
                         city AS ""Город"",
                         street AS ""Улица"",
                         building_number AS ""Дом""
                  FROM addresses
                  ORDER BY country, city, street, building_number;").DefaultView;
            StatusUiHelper.SetStatus(StatusText, "Справочник адресов загружен.", StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить адреса.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка адресов", ex);
        }
    }

    private static int? SelectedId(DataGrid? grid, string idColumn)
        => DataGridHelper.GetSelectedId(grid?.SelectedItem, idColumn);

    private void OnRefreshCaptains(object sender, RoutedEventArgs e) => LoadCaptains();
    private void OnRefreshPorts(object sender, RoutedEventArgs e) => LoadPorts();
    private void OnRefreshAddresses(object sender, RoutedEventArgs e) => LoadAddresses();

    private void OnAddCaptain(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var dlg = new CaptainEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadCaptains();
    }

    private void OnEditCaptain(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(CaptainsGrid, "captain_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите капитана."); return; }
        var dlg = new CaptainEditDialog(id.Value) { Owner = this };
        if (dlg.ShowDialog() == true) LoadCaptains();
    }

    private void OnDeleteCaptain(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(CaptainsGrid, "captain_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите капитана."); return; }
        if (!WpfMessageService.Confirm("Удалить выбранного капитана?")) return;
        try
        {
            Db.Execute("DELETE FROM captains WHERE captain_id=@id;", new NpgsqlParameter("@id", id.Value));
            LoadCaptains();
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Удаление капитана", ex);
        }
    }

    private void OnAddPort(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var dlg = new PortEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadPorts();
    }

    private void OnEditPort(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(PortsGrid, "port_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите порт."); return; }
        var dlg = new PortEditDialog(id.Value) { Owner = this };
        if (dlg.ShowDialog() == true) LoadPorts();
    }

    private void OnDeletePort(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(PortsGrid, "port_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите порт."); return; }
        if (!WpfMessageService.Confirm("Удалить выбранный порт?")) return;
        try
        {
            Db.Execute("DELETE FROM ports WHERE port_id=@id;", new NpgsqlParameter("@id", id.Value));
            LoadPorts();
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Удаление порта", ex);
        }
    }

    private void OnAddAddress(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var dlg = new AddressEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadAddresses();
    }

    private void OnEditAddress(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(AddressesGrid, "address_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите адрес."); return; }
        var dlg = new AddressEditDialog(id.Value) { Owner = this };
        if (dlg.ShowDialog() == true) LoadAddresses();
    }

    private void OnDeleteAddress(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = SelectedId(AddressesGrid, "address_id");
        if (!id.HasValue) { WpfMessageService.ShowInfo("Выберите адрес."); return; }
        if (!WpfMessageService.Confirm("Удалить выбранный адрес?")) return;
        try
        {
            Db.Execute("DELETE FROM addresses WHERE address_id=@id;", new NpgsqlParameter("@id", id.Value));
            LoadAddresses();
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Удаление адреса", ex);
        }
    }
}
