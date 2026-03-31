using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно управления рейсами: фильтрация по судну и диапазону дат, CRUD, просмотр деталей строки.
/// </summary>
public partial class ShipmentsWindow : Window
{
    private CrudWindowHelper _crud = null!;

    public ShipmentsWindow()
    {
        InitializeComponent();
        FromDate.SelectedDate = DateTime.Today.AddMonths(-1);
        ToDate.SelectedDate = DateTime.Today;
        _crud = new CrudWindowHelper(this, AddButton, EditButton, DeleteButton);
        Loaded += (_, _) => { LoadShipsFilter(); LoadShipments(); };
    }

    private void LoadShipsFilter()
    {
        var dtShips = Db.Query(ShipmentQueries.ShipsForFilterCombo);
        var allRow = dtShips.NewRow();
        allRow["ship_id"] = -1;
        allRow["reg_number"] = "Все суда";
        allRow["name"] = "";
        dtShips.Rows.InsertAt(allRow, 0);
        ShipsCombo.ItemsSource = dtShips.DefaultView;
        ShipsCombo.SelectedIndex = 0;
    }

    private void LoadShipments()
    {
        ActionBar.IsEnabled = false;
        FilterBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, UiText.Status.LoadShipments);
        try
        {
            var selected = ShipsCombo.SelectedValue;
            var shipId = selected == null ? -1 : Convert.ToInt32(selected);
            var from = (FromDate.SelectedDate ?? DateTime.Today.AddMonths(-1)).Date;
            var to = (ToDate.SelectedDate ?? DateTime.Today).Date;

            var parameters = new List<NpgsqlParameter> { new("@from", from), new("@to", to) };
            var where = " WHERE \"Дата отправления\" >= @from AND \"Дата отправления\" <= @to ";

            if (shipId != -1)
            {
                where += " AND shipment_id IN (SELECT shipment_id FROM shipments WHERE ship_id = @ship_id) ";
                parameters.Add(new NpgsqlParameter("@ship_id", shipId));
            }

            var sql = ShipmentQueries.ShipmentsFullInfoPrefix + where + ShipmentQueries.ShipmentsOrderByDeparture;
            var dt = Db.Query(sql, parameters.ToArray());
            DataGridHelper.NormalizeColumnNamesForWpf(dt);
            Grid.ItemsSource = dt.DefaultView;
            StatusUiHelper.SetStatus(
                StatusText,
                dt.Rows.Count == 0
                    ? "По выбранным условиям рейсы не найдены. Измените фильтры или нажмите «Сбросить фильтры»."
                    : $"Загружено рейсов: {dt.Rows.Count}.",
                dt.Rows.Count == 0 ? StatusKind.Warning : StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить список рейсов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка списка рейсов", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
            FilterBar.IsEnabled = true;
        }
    }

    private void OnRefresh(object sender, RoutedEventArgs e) => LoadShipments();

    private void OnResetFilters(object sender, RoutedEventArgs e)
    {
        ShipsCombo.SelectedIndex = 0;
        FromDate.SelectedDate = DateTime.Today.AddMonths(-1);
        ToDate.SelectedDate = DateTime.Today;
        LoadShipments();
    }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var dlg = new Dialogs.ShipmentEditDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(ShipmentQueries.InsertShipment,
                new NpgsqlParameter("@ship_id", v.ShipId),
                new NpgsqlParameter("@origin_port_id", v.OriginPortId),
                new NpgsqlParameter("@destination_port_id", v.DestinationPortId),
                new NpgsqlParameter("@departure_date", v.DepartureDate),
                new NpgsqlParameter("@arrive_date", v.ArriveDate.HasValue ? v.ArriveDate.Value.Date : (object)DBNull.Value),
                new NpgsqlParameter("@customs_value", v.CustomsValue.HasValue ? v.CustomsValue.Value : (object)DBNull.Value),
                new NpgsqlParameter("@custom_clearance", v.CustomClearance));
            LoadShipments();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Добавление рейса", ex); }
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "shipment_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectShipment); return; }
        var dlg = new Dialogs.ShipmentEditDialog(id.Value) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(ShipmentQueries.UpdateShipment,
                new NpgsqlParameter("@shipment_id", id.Value),
                new NpgsqlParameter("@ship_id", v.ShipId),
                new NpgsqlParameter("@origin_port_id", v.OriginPortId),
                new NpgsqlParameter("@destination_port_id", v.DestinationPortId),
                new NpgsqlParameter("@departure_date", v.DepartureDate),
                new NpgsqlParameter("@arrive_date", v.ArriveDate.HasValue ? v.ArriveDate.Value.Date : (object)DBNull.Value),
                new NpgsqlParameter("@customs_value", v.CustomsValue.HasValue ? v.CustomsValue.Value : (object)DBNull.Value),
                new NpgsqlParameter("@custom_clearance", v.CustomClearance));
            LoadShipments();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Изменение рейса", ex); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "shipment_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectShipment); return; }
        if (!WpfMessageService.Confirm("Удалить выбранный рейс?")) return;
        try
        {
            Db.Execute(ShipmentQueries.DeleteShipment, new NpgsqlParameter("@id", id.Value));
            LoadShipments();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Удаление рейса", ex); }
    }

    private void OnGridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(Grid.SelectedItem, "Детали рейса");

}
