using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления / изменения груза (таблица <c>cargo</c>).</summary>
public partial class CargoEditDialog : Window
{
    private readonly int? _cargoId;
    private int _shipmentId;
    /// <summary>Результат ввода пользователя. Доступен после <c>ShowDialog() == true</c>.</summary>
    public CargoEditValues Values { get; private set; }

    /// <param name="id">ID рейса (режим добавления) или ID груза (режим редактирования).</param>
    /// <param name="editExisting">Если <c>true</c> — <paramref name="id"/> интерпретируется как <c>cargo_id</c>.</param>
    public CargoEditDialog(int id, bool editExisting = false)
    {
        if (editExisting)
        { _cargoId = id; _shipmentId = 0; }
        else
        { _cargoId = null; _shipmentId = id; }

        InitializeComponent();
        Title = _cargoId.HasValue ? "Изменить груз" : "Добавить груз";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            SenderBox.ItemsSource   = Db.Query("SELECT sender_id, sender_name FROM senders ORDER BY sender_name;").DefaultView;
            ConsigneeBox.ItemsSource= Db.Query("SELECT consignee_id, consignee_name FROM consignees ORDER BY consignee_name;").DefaultView;
            UnitBox.ItemsSource     = Db.Query("SELECT unit_id, unit_name FROM units ORDER BY unit_name;").DefaultView;
            Values = new CargoEditValues { ShipmentId = _shipmentId };

            if (_cargoId.HasValue)
            {
                var dt = Db.Query(
                    "SELECT cargo_id, shipment_id, sender_id, consignee_id, cargo_number, cargo_name, unit_id, declared_value, insured_value, custom_value, quantity, comment FROM cargo WHERE cargo_id=@id;",
                    new NpgsqlParameter("@id", _cargoId.Value));
                if (dt.Rows.Count == 0) throw new InvalidOperationException("Груз не найден в БД.");
                var r = dt.Rows[0];
                Values = new CargoEditValues { CargoId = Convert.ToInt32(r["cargo_id"]), ShipmentId = Convert.ToInt32(r["shipment_id"]) };
                SenderBox.SelectedValue    = Convert.ToInt32(r["sender_id"]);
                ConsigneeBox.SelectedValue = Convert.ToInt32(r["consignee_id"]);
                CargoNumberBox.Text = Convert.ToString(r["cargo_number"]) ?? "0";
                CargoNameBox.Text   = Convert.ToString(r["cargo_name"]) ?? "";
                UnitBox.SelectedValue = Convert.ToInt32(r["unit_id"]);
                DeclaredBox.Text  = Convert.ToString(r["declared_value"]) ?? "0";
                InsuredBox.Text   = Convert.ToString(r["insured_value"]) ?? "0";
                CustomBox.Text    = Convert.ToString(r["custom_value"]) ?? "0";
                QuantityBox.Text  = Convert.ToString(r["quantity"]) ?? "0";
                CommentBox.Text   = r["comment"] == DBNull.Value ? "" : Convert.ToString(r["comment"]) ?? "";
            }
        }
        catch (Exception ex) { WpfMessageService.ShowError(ex.Message); DialogResult = false; }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (SenderBox.SelectedValue == null || ConsigneeBox.SelectedValue == null || UnitBox.SelectedValue == null)
        { WpfMessageService.ShowInfo("Выберите отправителя, получателя и единицу измерения.", "Проверка"); return; }
        if (string.IsNullOrWhiteSpace(CargoNameBox.Text))
        { WpfMessageService.ShowInfo("Введите наименование груза.", "Проверка"); return; }
        if (!int.TryParse(CargoNumberBox.Text.Trim(), out var cargoNum) || cargoNum < 0 ||
            !decimal.TryParse(DeclaredBox.Text.Trim(), out var declared) ||
            !decimal.TryParse(InsuredBox.Text.Trim(),  out var insured)  ||
            !decimal.TryParse(CustomBox.Text.Trim(),   out var custom)   ||
            !decimal.TryParse(QuantityBox.Text.Trim(), out var quantity)  ||
            declared < 0 || insured < 0 || custom < 0 || quantity < 0)
        { WpfMessageService.ShowInfo("Числовые поля должны содержать значения >= 0.", "Проверка"); return; }

        Values = new CargoEditValues
        {
            CargoId       = _cargoId ?? Values.CargoId,
            ShipmentId    = Values.ShipmentId,
            SenderId      = Convert.ToInt32(SenderBox.SelectedValue),
            ConsigneeId   = Convert.ToInt32(ConsigneeBox.SelectedValue),
            CargoNumber   = cargoNum,
            CargoName     = CargoNameBox.Text.Trim(),
            UnitId        = Convert.ToInt32(UnitBox.SelectedValue),
            DeclaredValue = declared,
            InsuredValue  = insured,
            CustomValue   = custom,
            Quantity      = quantity,
            Comment       = CommentBox.Text
        };
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) { DialogResult = false; }
}
