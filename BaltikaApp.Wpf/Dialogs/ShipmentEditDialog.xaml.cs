using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления / изменения рейса (таблица <c>shipments</c>).</summary>
public partial class ShipmentEditDialog : Window
{
    private readonly int? _shipmentId;
    /// <summary>Результат ввода пользователя. Доступен после <c>ShowDialog() == true</c>.</summary>
    public ShipmentEditValues Values { get; private set; }

    public ShipmentEditDialog(int? shipmentId = null)
    {
        _shipmentId = shipmentId;
        InitializeComponent();
        Title = shipmentId.HasValue ? "Изменить рейс" : "Добавить рейс";
        DeparturePicker.SelectedDate = DateTime.Today;
        ArrivePicker.SelectedDate    = DateTime.Today;
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var ships = Db.Query("SELECT ship_id, reg_number FROM ships ORDER BY reg_number;");
            ShipBox.ItemsSource = ships.DefaultView;
            LoadPorts();

            if (_shipmentId.HasValue)
            {
                var dt = Db.Query(
                    "SELECT ship_id, origin_port_id, destination_port_id, departure_date, arrive_date, customs_value, custom_clearance FROM shipments WHERE shipment_id=@id;",
                    new NpgsqlParameter("@id", _shipmentId.Value));
                if (dt.Rows.Count == 0) throw new InvalidOperationException("Рейс не найден в БД.");
                var r = dt.Rows[0];
                ShipBox.SelectedValue    = Convert.ToInt32(r["ship_id"]);
                OriginBox.SelectedValue  = Convert.ToInt32(r["origin_port_id"]);
                DestBox.SelectedValue    = Convert.ToInt32(r["destination_port_id"]);
                DeparturePicker.SelectedDate = Convert.ToDateTime(r["departure_date"]).Date;
                if (r["arrive_date"] != DBNull.Value)
                { ArriveCheck.IsChecked = true; ArrivePicker.SelectedDate = Convert.ToDateTime(r["arrive_date"]).Date; }
                if (r["customs_value"] != DBNull.Value)
                { CustomsCheck.IsChecked = true; CustomsBox.Text = Convert.ToString(r["customs_value"]) ?? "0"; }
                ClearanceCheck.IsChecked = Convert.ToBoolean(r["custom_clearance"]);
            }
        }
        catch (Exception ex) { WpfMessageService.ShowError(ex.Message); DialogResult = false; }
    }

    private void LoadPorts()
    {
        var ports = Db.Query("SELECT port_id, port_name FROM ports ORDER BY port_name;");
        OriginBox.ItemsSource = ports.Copy().DefaultView;
        DestBox.ItemsSource = ports.DefaultView;
    }

    private void OnAddPort(object sender, RoutedEventArgs e)
    {
        if (!ConnectionManager.IsWriter) { WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter); return; }
        var dlg = new PortEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadPorts();
    }

    private void OnArriveChecked(object sender, RoutedEventArgs e)
        => ArrivePicker.IsEnabled = ArriveCheck.IsChecked == true;

    private void OnCustomsChecked(object sender, RoutedEventArgs e)
        => CustomsBox.IsEnabled = CustomsCheck.IsChecked == true;

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (ShipBox.SelectedValue == null || OriginBox.SelectedValue == null || DestBox.SelectedValue == null)
        { WpfMessageService.ShowInfo("Выберите судно и порты.", "Проверка"); return; }
        if (DeparturePicker.SelectedDate == null)
        { WpfMessageService.ShowInfo("Укажите дату отправления.", "Проверка"); return; }

        decimal? customs = null;
        if (CustomsCheck.IsChecked == true)
        {
            if (!decimal.TryParse(CustomsBox.Text.Trim(), out var cv) || cv < 0)
            { WpfMessageService.ShowInfo("Таможенная стоимость — число >= 0.", "Проверка"); return; }
            customs = cv;
        }

        Values = new ShipmentEditValues
        {
            ShipId              = Convert.ToInt32(ShipBox.SelectedValue),
            OriginPortId        = Convert.ToInt32(OriginBox.SelectedValue),
            DestinationPortId   = Convert.ToInt32(DestBox.SelectedValue),
            DepartureDate       = DeparturePicker.SelectedDate!.Value.Date,
            ArriveDate          = ArriveCheck.IsChecked == true ? ArrivePicker.SelectedDate?.Date : null,
            CustomsValue        = customs,
            CustomClearance     = ClearanceCheck.IsChecked == true
        };
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) { DialogResult = false; }
}
