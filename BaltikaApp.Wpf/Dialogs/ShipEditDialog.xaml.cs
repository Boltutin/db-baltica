using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления / изменения судна (таблица <c>ships</c>).</summary>
public partial class ShipEditDialog : Window
{
    private readonly int? _shipId;

    /// <summary>Результат ввода пользователя. Доступен после <c>ShowDialog() == true</c>.</summary>
    public ShipEditValues Values { get; private set; }

    public ShipEditDialog(int? shipId = null)
    {
        _shipId = shipId;
        InitializeComponent();
        Title = shipId.HasValue ? "Изменить судно" : "Добавить судно";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadLookups();

            if (_shipId.HasValue)
            {
                var dt = Db.Query(
                    "SELECT reg_number, name, captain_id, type_id, dockyard_id, capacity, year_built, customs_value, home_port_id FROM ships WHERE ship_id=@id;",
                    new NpgsqlParameter("@id", _shipId.Value));
                if (dt.Rows.Count == 0) throw new InvalidOperationException("Судно не найдено в БД.");
                var r = dt.Rows[0];
                RegBox.Text      = Convert.ToString(r["reg_number"]) ?? "";
                NameBox.Text     = Convert.ToString(r["name"]) ?? "";
                TypeBox.SelectedValue    = Convert.ToInt32(r["type_id"]);
                CaptainBox.SelectedValue = Convert.ToInt32(r["captain_id"]);
                DockyardBox.SelectedValue= Convert.ToInt32(r["dockyard_id"]);
                PortBox.SelectedValue    = Convert.ToInt32(r["home_port_id"]);
                CapacityBox.Text = Convert.ToString(r["capacity"]) ?? "0";
                YearBox.Text     = Convert.ToString(r["year_built"]) ?? "1980";
                CustomsBox.Text  = Convert.ToString(r["customs_value"]) ?? "0";
            }
        }
        catch (Exception ex) { WpfMessageService.ShowError(ex.Message); DialogResult = false; }
    }

    private void LoadLookups()
    {
        TypeBox.ItemsSource = Db.Query("SELECT type_id, type_name FROM ship_types ORDER BY type_name;").DefaultView;
        CaptainBox.ItemsSource = Db.Query("SELECT captain_id, full_name FROM captains ORDER BY full_name;").DefaultView;
        DockyardBox.ItemsSource = Db.Query("SELECT dockyard_id, name FROM dockyards ORDER BY name;").DefaultView;
        PortBox.ItemsSource = Db.Query("SELECT port_id, port_name FROM ports ORDER BY port_name;").DefaultView;
    }

    private void OnAddCaptain(object sender, RoutedEventArgs e)
    {
        if (!ConnectionManager.IsWriter) { WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter); return; }
        var dlg = new CaptainEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadLookups();
    }

    private void OnAddPort(object sender, RoutedEventArgs e)
    {
        if (!ConnectionManager.IsWriter) { WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter); return; }
        var dlg = new PortEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadLookups();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(RegBox.Text) || string.IsNullOrWhiteSpace(NameBox.Text))
        { WpfMessageService.ShowInfo("Заполните рег. номер и название.", "Проверка"); return; }
        if (TypeBox.SelectedValue == null || CaptainBox.SelectedValue == null || DockyardBox.SelectedValue == null || PortBox.SelectedValue == null)
        { WpfMessageService.ShowInfo("Заполните: тип судна, капитан, верфь, порт приписки.", "Проверка"); return; }
        if (!int.TryParse(CapacityBox.Text.Trim(), out var cap) || cap < 0)
        { WpfMessageService.ShowInfo("Грузоподъёмность — целое число >= 0.", "Проверка"); return; }
        if (!int.TryParse(YearBox.Text.Trim(), out var year) || year < 1900 || year > 2100)
        { WpfMessageService.ShowInfo("Год постройки: 1900–2100.", "Проверка"); return; }
        if (!decimal.TryParse(CustomsBox.Text.Trim(), out var customs) || customs < 0)
        { WpfMessageService.ShowInfo("Таможенная стоимость — число >= 0.", "Проверка"); return; }

        Values = new ShipEditValues
        {
            RegNumber    = RegBox.Text.Trim(),
            Name         = NameBox.Text.Trim(),
            TypeId       = Convert.ToInt32(TypeBox.SelectedValue),
            CaptainId    = Convert.ToInt32(CaptainBox.SelectedValue),
            DockyardId   = Convert.ToInt32(DockyardBox.SelectedValue),
            HomePortId   = Convert.ToInt32(PortBox.SelectedValue),
            Capacity     = cap,
            YearBuilt    = year,
            CustomsValue = customs
        };
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) { DialogResult = false; }
}
