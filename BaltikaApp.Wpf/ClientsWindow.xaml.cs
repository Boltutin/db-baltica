using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно клиентов: вкладки «Отправители», «Получатели» и «Активность».
/// CRUD доступен только для первых двух вкладок.
/// </summary>
public partial class ClientsWindow : Window
{
    private CrudWindowHelper _crud = null!;

    public ClientsWindow()
    {
        InitializeComponent();
        _crud = new CrudWindowHelper(this, AddButton, EditButton, DeleteButton);
        Loaded += (_, _) => { LoadAll(); UpdateCrudByTab(); };
    }

    private void LoadAll()
    {
        ActionBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, UiText.Status.LoadClients);
        try
        {
            var senders = Db.Query(ClientsQueries.SendersGrid);
            var consignees = Db.Query(ClientsQueries.ConsigneesGrid);
            var activity = Db.Query(ClientsQueries.ClientsActivity);
            DataGridHelper.NormalizeColumnNamesForWpf(senders);
            DataGridHelper.NormalizeColumnNamesForWpf(consignees);
            DataGridHelper.NormalizeColumnNamesForWpf(activity);
            SendersGrid.ItemsSource    = senders.DefaultView;
            ConsigneesGrid.ItemsSource = consignees.DefaultView;
            ActivityGrid.ItemsSource   = activity.DefaultView;
            StatusUiHelper.SetStatus(StatusText, BuildStatus(senders, consignees, activity), StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить данные клиентов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка данных клиентов", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
        }
    }

    private void UpdateCrudByTab()
    {
        var allowed = ConnectionManager.IsWriter && (Tabs.SelectedIndex == 0 || Tabs.SelectedIndex == 1);
        AddButton.IsEnabled = allowed;
        EditButton.IsEnabled = allowed;
        DeleteButton.IsEnabled = allowed;
    }

    private int? GetSelectedSenderId()
        => Tabs.SelectedIndex == 0 ? DataGridHelper.GetSelectedId(SendersGrid.SelectedItem, "sender_id") : null;

    private int? GetSelectedConsigneeId()
        => Tabs.SelectedIndex == 1 ? DataGridHelper.GetSelectedId(ConsigneesGrid.SelectedItem, "consignee_id") : null;

    private void OnTabChanged(object sender, SelectionChangedEventArgs e) => UpdateCrudByTab();
    private void OnRefresh(object sender, RoutedEventArgs e) { LoadAll(); UpdateCrudByTab(); }

    private void OnAdd(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        try
        {
            if (Tabs.SelectedIndex == 0)
            {
                var dlg = new Dialogs.SenderEditDialog { Owner = this };
                if (dlg.ShowDialog() != true) return;
                var v = dlg.Values;
                Db.Execute(ClientsQueries.InsertSender,
                    new NpgsqlParameter("@name", v.SenderName),
                    new NpgsqlParameter("@inn", v.Inn),
                    new NpgsqlParameter("@bank_id", v.BankId),
                    new NpgsqlParameter("@address_id", v.AddressId));
            }
            else if (Tabs.SelectedIndex == 1)
            {
                var dlg = new Dialogs.ConsigneeEditDialog { Owner = this };
                if (dlg.ShowDialog() != true) return;
                var v = dlg.Values;
                Db.Execute(ClientsQueries.InsertConsignee,
                    new NpgsqlParameter("@name", v.ConsigneeName),
                    new NpgsqlParameter("@inn", v.Inn),
                    new NpgsqlParameter("@bank_id", v.BankId),
                    new NpgsqlParameter("@address_id", v.AddressId));
            }
            else { WpfMessageService.ShowInfo(UiText.Common.CrudOnlyForTabs); return; }
            LoadAll();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Добавление клиента", ex); }
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        try
        {
            if (Tabs.SelectedIndex == 0)
            {
                var id = GetSelectedSenderId();
                if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectSender); return; }
                var dlg = new Dialogs.SenderEditDialog(id.Value) { Owner = this };
                if (dlg.ShowDialog() != true) return;
                var v = dlg.Values;
                Db.Execute(ClientsQueries.UpdateSender,
                    new NpgsqlParameter("@id", id.Value),
                    new NpgsqlParameter("@name", v.SenderName),
                    new NpgsqlParameter("@inn", v.Inn),
                    new NpgsqlParameter("@bank_id", v.BankId),
                    new NpgsqlParameter("@address_id", v.AddressId));
            }
            else if (Tabs.SelectedIndex == 1)
            {
                var id = GetSelectedConsigneeId();
                if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectConsignee); return; }
                var dlg = new Dialogs.ConsigneeEditDialog(id.Value) { Owner = this };
                if (dlg.ShowDialog() != true) return;
                var v = dlg.Values;
                Db.Execute(ClientsQueries.UpdateConsignee,
                    new NpgsqlParameter("@id", id.Value),
                    new NpgsqlParameter("@name", v.ConsigneeName),
                    new NpgsqlParameter("@inn", v.Inn),
                    new NpgsqlParameter("@bank_id", v.BankId),
                    new NpgsqlParameter("@address_id", v.AddressId));
            }
            else { WpfMessageService.ShowInfo(UiText.Common.CrudOnlyForTabs); return; }
            LoadAll();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Изменение клиента", ex); }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (!CrudWindowHelper.RequireWriter()) return;
        try
        {
            if (Tabs.SelectedIndex == 0)
            {
                var id = GetSelectedSenderId();
                if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectSender); return; }
                if (!WpfMessageService.Confirm("Удалить выбранного отправителя?")) return;
                Db.Execute(ClientsQueries.DeleteSender, new NpgsqlParameter("@id", id.Value));
            }
            else if (Tabs.SelectedIndex == 1)
            {
                var id = GetSelectedConsigneeId();
                if (id == null) { WpfMessageService.ShowInfo(UiText.Common.SelectConsignee); return; }
                if (!WpfMessageService.Confirm("Удалить выбранного получателя?")) return;
                Db.Execute(ClientsQueries.DeleteConsignee, new NpgsqlParameter("@id", id.Value));
            }
            else { WpfMessageService.ShowInfo(UiText.Common.CrudOnlyForTabs); return; }
            LoadAll();
        }
        catch (Exception ex) { WpfMessageService.ShowOperationError("Удаление клиента", ex); }
    }

    private void OnSenderDetails(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(SendersGrid.SelectedItem, "Детали отправителя");

    private void OnConsigneeDetails(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(ConsigneesGrid.SelectedItem, "Детали получателя");

    private void OnActivityDetails(object sender, System.Windows.Input.MouseButtonEventArgs e)
        => DataGridHelper.ShowRowDetails(ActivityGrid.SelectedItem, "Детали активности клиента");

    private static string BuildStatus(DataTable senders, DataTable consignees, DataTable activity)
    {
        if (senders.Rows.Count == 0 && consignees.Rows.Count == 0)
            return "Справочники отправителей и получателей пусты. Добавьте первую запись.";
        return $"Отправителей: {senders.Rows.Count}, получателей: {consignees.Rows.Count}, строк активности: {activity.Rows.Count}.";
    }
}
