using System;
using System.Data;
using System.Windows;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно управления судами: просмотр списка, добавление, изменение и удаление записей.
/// </summary>
public partial class ShipsWindow : Window
{
    private CrudWindowHelper _crud = null!;

    public ShipsWindow()
    {
        InitializeComponent();
        _crud = new CrudWindowHelper(this, AddButton, EditButton, DeleteButton);
        Loaded += (_, _) => LoadShips();
    }

    private void LoadShips()
    {
        ActionBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, UiText.Status.LoadShips);
        try
        {
            var dt = Db.Query(ShipQueries.ListShipsExtended);
            DataGridHelper.NormalizeColumnNamesForWpf(dt);
            Grid.ItemsSource = dt.DefaultView;
            UpdateEmptyState(dt, "судов");
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить список судов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка списка судов", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
        }
    }

    private void OnRefresh(object sender, RoutedEventArgs e) => LoadShips();

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var dlg = new Dialogs.ShipEditDialog { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(ShipQueries.InsertShip,
                new NpgsqlParameter("@reg", v.RegNumber),
                new NpgsqlParameter("@name", v.Name),
                new NpgsqlParameter("@captain", v.CaptainId),
                new NpgsqlParameter("@type", v.TypeId),
                new NpgsqlParameter("@dockyard", v.DockyardId),
                new NpgsqlParameter("@capacity", v.Capacity),
                new NpgsqlParameter("@year_built", v.YearBuilt),
                new NpgsqlParameter("@customs_value", v.CustomsValue),
                new NpgsqlParameter("@home_port_id", v.HomePortId));
            LoadShips();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Добавление судна", ex); }
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "ship_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectShip); return; }
        var dlg = new Dialogs.ShipEditDialog(id.Value) { Owner = this };
        if (dlg.ShowDialog() != true) return;
        var v = dlg.Values;
        try
        {
            Db.Execute(ShipQueries.UpdateShip,
                new NpgsqlParameter("@ship_id", id.Value),
                new NpgsqlParameter("@reg", v.RegNumber),
                new NpgsqlParameter("@name", v.Name),
                new NpgsqlParameter("@captain", v.CaptainId),
                new NpgsqlParameter("@type", v.TypeId),
                new NpgsqlParameter("@dockyard", v.DockyardId),
                new NpgsqlParameter("@capacity", v.Capacity),
                new NpgsqlParameter("@year_built", v.YearBuilt),
                new NpgsqlParameter("@customs_value", v.CustomsValue),
                new NpgsqlParameter("@home_port_id", v.HomePortId));
            LoadShips();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Изменение судна", ex); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        var id = DataGridHelper.GetSelectedId(Grid.SelectedItem, "ship_id");
        if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectShip); return; }
        if (!WpfMessageService.Confirm("Удалить выбранное судно?")) return;
        try
        {
            Db.Execute(ShipQueries.DeleteShip, new NpgsqlParameter("@id", id.Value));
            LoadShips();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Удаление судна", ex); }
    }

    private void OnDetails(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(Grid.SelectedItem, "Детали судна");

    private void UpdateEmptyState(DataTable dt, string entityName)
    {
        StatusUiHelper.SetStatus(
            StatusText,
            dt.Rows.Count == 0
                ? $"Данные не найдены: список {entityName} пуст. Добавьте первую запись."
                : $"Загружено записей: {dt.Rows.Count}.",
            dt.Rows.Count == 0 ? StatusKind.Warning : StatusKind.Success);
    }
}
