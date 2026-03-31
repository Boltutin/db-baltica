using System;
using System.Windows;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;
using Npgsql;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно рейсов за период: вызывает хранимую функцию БД
/// <c>get_shipments_by_period(date, date)</c> с выбранными датами.
/// </summary>
public partial class ShipmentsByPeriodWindow : Window
{
    public ShipmentsByPeriodWindow()
    {
        InitializeComponent();
        StartDate.SelectedDate = DateTime.Today.AddMonths(-1);
        EndDate.SelectedDate = DateTime.Today;
    }

    private void OnFind(object sender, RoutedEventArgs e)
    {
        ActionBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, UiText.Status.SearchPeriod);
        try
        {
            var pStart = new NpgsqlParameter("@start", NpgsqlTypes.NpgsqlDbType.Date)
            {
                Value = (StartDate.SelectedDate ?? DateTime.Today.AddMonths(-1)).Date
            };
            var pEnd = new NpgsqlParameter("@end", NpgsqlTypes.NpgsqlDbType.Date)
            {
                Value = (EndDate.SelectedDate ?? DateTime.Today).Date
            };

            var dt = Db.Query(
                "SELECT * FROM get_shipments_by_period(@start::date, @end::date);",
                pStart,
                pEnd);

            DataGridHelper.NormalizeColumnNamesForWpf(dt);
            Grid.ItemsSource = dt.DefaultView;
            StatusUiHelper.SetStatus(
                StatusText,
                dt.Rows.Count == 0
                    ? "Рейсы за выбранный период не найдены. Попробуйте расширить диапазон дат."
                    : $"Найдено рейсов: {dt.Rows.Count}.",
                dt.Rows.Count == 0 ? StatusKind.Warning : StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось выполнить поиск рейсов.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Поиск рейсов по периоду", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
        }
    }
}
