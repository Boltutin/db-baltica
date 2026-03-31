using System;
using System.Data;
using System.Windows;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно управления грузами: фильтрация по рейсу, CRUD, просмотр деталей записи.
/// </summary>
public partial class CargoWindow : Window
{
    private CrudWindowHelper _crud = null!;

    public CargoWindow()
    {
        InitializeComponent();
        _crud = new CrudWindowHelper(this, AddButton, EditButton, DeleteButton);
        Loaded += (_, _) => { LoadShipmentsFilter(); LoadCargo(); };
    }

    private void LoadShipmentsFilter()
    {
        var dt = Db.Query(CargoQueries.ShipmentsForFilterCombo);
        var allRow = dt.NewRow();
        allRow["shipment_id"] = -1;
        allRow["caption"] = "Все рейсы";
        dt.Rows.InsertAt(allRow, 0);
        ShipmentsCombo.ItemsSource = dt.DefaultView;
        ShipmentsCombo.SelectedIndex = 0;
    }

    private void LoadCargo()
    {
        ActionBar.IsEnabled = false;
        FilterBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, UiText.Status.LoadCargo);
        try
        {
            var shipmentId = ShipmentsCombo.SelectedValue == null ? -1 : Convert.ToInt32(ShipmentsCombo.SelectedValue);
            var dt = shipmentId == -1
                ? Db.Query(CargoQueries.AllCargo)
                : Db.Query(CargoQueries.CargoByShipment, new NpgsqlParameter("@shipment_id", shipmentId));
            DataGridHelper.NormalizeColumnNamesForWpf(dt);
            Grid.ItemsSource = dt.DefaultView;
            StatusUiHelper.SetStatus(
                StatusText,
                dt.Rows.Count == 0
                    ? "По выбранному фильтру грузы не найдены. Измените выбор рейса или сбросьте фильтр."
                    : $"Загружено грузов: {dt.Rows.Count}.",
                dt.Rows.Count == 0 ? StatusKind.Warning : StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить список грузов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка списка грузов", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
            FilterBar.IsEnabled = true;
        }
    }

    private void OnRefresh(object sender, RoutedEventArgs e) => LoadCargo();

    private void OnResetFilter(object sender, RoutedEventArgs e)
    {
        ShipmentsCombo.SelectedIndex = 0;
        LoadCargo();
    }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var shipmentId = ShipmentsCombo.SelectedValue == null ? -1 : Convert.ToInt32(ShipmentsCombo.SelectedValue);
        if (shipmentId == -1) { WpfMessageService.ShowInfo("Выберите конкретный рейс для добавления грузов."); return; }
        var dlg = new Dialogs.CargoEditDialog(shipmentId) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(CargoQueries.InsertCargo,
                new NpgsqlParameter("@shipment_id", v.ShipmentId),
                new NpgsqlParameter("@sender_id", v.SenderId),
                new NpgsqlParameter("@consignee_id", v.ConsigneeId),
                new NpgsqlParameter("@cargo_number", v.CargoNumber),
                new NpgsqlParameter("@cargo_name", v.CargoName),
                new NpgsqlParameter("@unit_id", v.UnitId),
                new NpgsqlParameter("@declared_value", v.DeclaredValue),
                new NpgsqlParameter("@insured_value", v.InsuredValue),
                new NpgsqlParameter("@custom_value", v.CustomValue),
                new NpgsqlParameter("@quantity", v.Quantity),
                new NpgsqlParameter("@comment", string.IsNullOrWhiteSpace(v.Comment) ? (object)DBNull.Value : v.Comment));
            LoadCargo();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Добавление груза", ex); }
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "cargo_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectCargo); return; }
        var dlg = new Dialogs.CargoEditDialog(id.Value, editExisting: true) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(CargoQueries.UpdateCargo,
                new NpgsqlParameter("@cargo_id", v.CargoId),
                new NpgsqlParameter("@sender_id", v.SenderId),
                new NpgsqlParameter("@consignee_id", v.ConsigneeId),
                new NpgsqlParameter("@cargo_number", v.CargoNumber),
                new NpgsqlParameter("@cargo_name", v.CargoName),
                new NpgsqlParameter("@unit_id", v.UnitId),
                new NpgsqlParameter("@declared_value", v.DeclaredValue),
                new NpgsqlParameter("@insured_value", v.InsuredValue),
                new NpgsqlParameter("@custom_value", v.CustomValue),
                new NpgsqlParameter("@quantity", v.Quantity),
                new NpgsqlParameter("@comment", string.IsNullOrWhiteSpace(v.Comment) ? (object)DBNull.Value : v.Comment));
            LoadCargo();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Изменение груза", ex); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "cargo_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectCargo); return; }
        if (!WpfMessageService.Confirm("Удалить выбранный груз?")) return;
        try
        {
            Db.Execute(CargoQueries.DeleteCargo, new NpgsqlParameter("@id", id.Value));
            LoadCargo();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Удаление груза", ex); }
    }

    private void OnDetails(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(Grid.SelectedItem, "Детали груза");

}
