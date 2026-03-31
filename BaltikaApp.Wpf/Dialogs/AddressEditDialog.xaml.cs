using System;
using System.Windows;
using BaltikaApp.Data;
using Npgsql;

namespace BaltikaApp.Wpf.Dialogs;

/// <summary>Диалог добавления/редактирования адреса.</summary>
public partial class AddressEditDialog : Window
{
    private readonly int? _addressId;

    public AddressEditDialog(int? addressId = null)
    {
        _addressId = addressId;
        InitializeComponent();
        Title = addressId.HasValue ? "Изменить адрес" : "Добавить адрес";
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (!_addressId.HasValue) return;
        try
        {
            var dt = Db.Query(
                "SELECT country, region, city, street, building_number FROM addresses WHERE address_id=@id;",
                new NpgsqlParameter("@id", _addressId.Value));
            if (dt.Rows.Count == 0) throw new InvalidOperationException("Адрес не найден.");

            var row = dt.Rows[0];
            CountryBox.Text = Convert.ToString(row["country"]) ?? string.Empty;
            RegionBox.Text = Convert.ToString(row["region"]) ?? string.Empty;
            CityBox.Text = Convert.ToString(row["city"]) ?? string.Empty;
            StreetBox.Text = Convert.ToString(row["street"]) ?? string.Empty;
            BuildingBox.Text = Convert.ToString(row["building_number"]) ?? string.Empty;
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Загрузка адреса", ex);
            DialogResult = false;
        }
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        var country = CountryBox.Text.Trim();
        var city = CityBox.Text.Trim();
        var street = StreetBox.Text.Trim();
        var building = BuildingBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(building))
        {
            WpfMessageService.ShowInfo("Заполните обязательные поля: страна, город, улица, дом.", "Проверка");
            return;
        }

        try
        {
            if (_addressId.HasValue)
            {
                Db.Execute(
                    @"UPDATE addresses
                      SET country=@country, region=@region, city=@city, street=@street, building_number=@building
                      WHERE address_id=@id;",
                    new NpgsqlParameter("@country", country),
                    new NpgsqlParameter("@region", string.IsNullOrWhiteSpace(RegionBox.Text) ? DBNull.Value : RegionBox.Text.Trim()),
                    new NpgsqlParameter("@city", city),
                    new NpgsqlParameter("@street", street),
                    new NpgsqlParameter("@building", building),
                    new NpgsqlParameter("@id", _addressId.Value));
            }
            else
            {
                Db.Execute(
                    @"INSERT INTO addresses (country, region, city, street, building_number)
                      VALUES (@country, @region, @city, @street, @building);",
                    new NpgsqlParameter("@country", country),
                    new NpgsqlParameter("@region", string.IsNullOrWhiteSpace(RegionBox.Text) ? DBNull.Value : RegionBox.Text.Trim()),
                    new NpgsqlParameter("@city", city),
                    new NpgsqlParameter("@street", street),
                    new NpgsqlParameter("@building", building));
            }

            DialogResult = true;
        }
        catch (Exception ex)
        {
            WpfMessageService.ShowOperationError("Сохранение адреса", ex);
        }
    }

    private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
}
