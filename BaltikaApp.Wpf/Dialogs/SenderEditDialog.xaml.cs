using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления / изменения отправителя (таблица <c>senders</c>).</summary>
public partial class SenderEditDialog : Window
{
    private readonly int? _senderId;
    /// <summary>Результат ввода пользователя. Доступен после <c>ShowDialog() == true</c>.</summary>
    public SenderEditValues Values { get; private set; }

    public SenderEditDialog(int? senderId = null)
    {
        _senderId = senderId;
        InitializeComponent();
        Title = senderId.HasValue ? "Изменить отправителя" : "Добавить отправителя";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            BankBox.ItemsSource    = Db.Query("SELECT bank_id, bank_name FROM banks ORDER BY bank_name;").DefaultView;
            LoadAddresses();

            if (_senderId.HasValue)
            {
                var dt = Db.Query("SELECT sender_name, inn_sender, bank_id, address_id FROM senders WHERE sender_id=@id;",
                    new NpgsqlParameter("@id", _senderId.Value));
                if (dt.Rows.Count == 0) throw new InvalidOperationException("Отправитель не найден в БД.");
                var r = dt.Rows[0];
                NameBox.Text = Convert.ToString(r["sender_name"]) ?? "";
                InnBox.Text  = Convert.ToString(r["inn_sender"]) ?? "";
                BankBox.SelectedValue    = Convert.ToInt32(r["bank_id"]);
                AddressBox.SelectedValue = Convert.ToInt32(r["address_id"]);
            }
        }
        catch (Exception ex) { WpfMessageService.ShowError(ex.Message); DialogResult = false; }
    }

    private void LoadAddresses()
    {
        AddressBox.ItemsSource = Db.Query(@"SELECT address_id,
                (country || ', ' || COALESCE(region || ', ', '') || city || ', ' || street || ' ' || building_number) AS caption
                FROM addresses ORDER BY country, city, street, building_number;").DefaultView;
    }

    private void OnAddAddress(object sender, RoutedEventArgs e)
    {
        if (!ConnectionManager.IsWriter) { WpfMessageService.ShowInfo(UiText.Common.OperationOnlyInWriter); return; }
        var dlg = new AddressEditDialog { Owner = this };
        if (dlg.ShowDialog() == true) LoadAddresses();
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        var inn  = InnBox.Text.Trim();
        if (string.IsNullOrEmpty(name)) { WpfMessageService.ShowInfo("Введите название.", "Проверка"); return; }
        if (string.IsNullOrEmpty(inn))  { WpfMessageService.ShowInfo("Введите ИНН.", "Проверка"); return; }
        if (BankBox.SelectedValue == null || AddressBox.SelectedValue == null)
        { WpfMessageService.ShowInfo("Выберите банк и адрес.", "Проверка"); return; }

        Values = new SenderEditValues
        {
            SenderName = name,
            Inn        = inn,
            BankId     = Convert.ToInt32(BankBox.SelectedValue),
            AddressId  = Convert.ToInt32(AddressBox.SelectedValue)
        };
        DialogResult = true;
    }

    private void OnCancel(object sender, RoutedEventArgs e) { DialogResult = false; }
}
