using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using ClosedXML.Excel;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Helpers;

namespace BaltikaApp.Wpf;

/// <summary>
/// Окно отчётов: набор предопределённых SQL-запросов с отображением в <see cref="DataGrid"/>,
/// экспортом в CSV/XLSX и печатью через <see cref="PrintDialog"/> и FlowDocument.
/// </summary>
public partial class ReportsWindow : Window
{
    private readonly Dictionary<string, string> _reportSql = new()
    {
        { "Суда по типам", ReportQueries.ShipsByType },
        { "Статистика по портам", ReportQueries.PortsStatistics },
        { "Активность судов", ReportQueries.ShipsActivity },
        { "Финансовая сводка по грузам", ReportQueries.CargoFinancialSummary },
        { "Клиенты", ReportQueries.ClientsActivityView },
        { "Рейсы по месяцам", ReportQueries.ShipmentsByMonth },
        { "Справочник: Капитаны", ReportQueries.RefCaptains },
        { "Справочник: Адреса", ReportQueries.RefAddresses },
        { "Справочник: Порты", ReportQueries.RefPorts },
        { "Справочник: Типы судов", ReportQueries.RefShipTypes },
        { "Справочник: Верфи", ReportQueries.RefDockyards },
        { "Справочник: Единицы измерения", ReportQueries.RefUnits },
        { "Справочник: Банки", ReportQueries.RefBanks }
    };

    private DataTable? _currentData;

    public ReportsWindow()
    {
        InitializeComponent();
        foreach (var key in _reportSql.Keys)
            ReportsCombo.Items.Add(key);
        if (ReportsCombo.Items.Count > 0)
            ReportsCombo.SelectedIndex = 0;
    }

    private void OnReportChanged(object sender, SelectionChangedEventArgs e)
    {
        LoadSelectedReport();
    }

    private void OnRefresh(object sender, RoutedEventArgs e)
    {
        LoadSelectedReport();
    }

    private void LoadSelectedReport()
    {
        var key = ReportsCombo.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(key))
            return;

        ActionBar.IsEnabled = false;
        StatusUiHelper.SetStatus(StatusText, $"{UiText.Status.LoadReportPrefix}{key}»...");
        try
        {
            _currentData = Db.Query(_reportSql[key]);
                DataGridHelper.NormalizeColumnNamesForWpf(_currentData);
            Grid.ItemsSource = _currentData.DefaultView;
            StatusUiHelper.SetStatus(
                StatusText,
                _currentData.Rows.Count == 0
                    ? "Отчёт сформирован, но данных по выбранным условиям нет. Уточните параметры и обновите."
                    : $"Отчёт загружен. Строк: {_currentData.Rows.Count}.",
                _currentData.Rows.Count == 0 ? StatusKind.Warning : StatusKind.Success);
        }
        catch (Exception ex)
        {
            StatusUiHelper.SetStatus(StatusText, "Не удалось загрузить отчёт.", StatusKind.Error);
            WpfMessageService.ShowOperationError("Загрузка отчёта", ex);
        }
        finally
        {
            ActionBar.IsEnabled = true;
        }
    }

    private void OnExportCsv(object sender, RoutedEventArgs e)
    {
        if (_currentData == null || _currentData.Rows.Count == 0)
        {
            WpfMessageService.ShowInfo("Экспорт недоступен: сначала загрузите отчёт с данными.");
            return;
        }

        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = BuildDefaultExportName("csv")
        };
        if (dlg.ShowDialog(this) != true)
            return;

        using var writer = new StreamWriter(dlg.FileName, false, new UTF8Encoding(true));
        writer.WriteLine(string.Join(";", GetHeaderValues(_currentData)));
        foreach (DataRow row in _currentData.Rows)
            writer.WriteLine(string.Join(";", GetRowValues(row)));
        WpfMessageService.ShowInfo("CSV успешно сохранен.");
    }

    private void OnExportXlsx(object sender, RoutedEventArgs e)
    {
        if (_currentData == null || _currentData.Rows.Count == 0)
        {
            WpfMessageService.ShowInfo("Экспорт недоступен: сначала загрузите отчёт с данными.");
            return;
        }

        var dlg = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel files (*.xlsx)|*.xlsx",
            FileName = BuildDefaultExportName("xlsx")
        };
        if (dlg.ShowDialog(this) != true)
            return;

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Отчет");
        for (int c = 0; c < _currentData.Columns.Count; c++)
            ws.Cell(1, c + 1).Value = _currentData.Columns[c].ColumnName;
        for (int r = 0; r < _currentData.Rows.Count; r++)
            for (int c = 0; c < _currentData.Columns.Count; c++)
                ws.Cell(r + 2, c + 1).Value = _currentData.Rows[r][c]?.ToString() ?? string.Empty;
        ws.Columns().AdjustToContents();
        wb.SaveAs(dlg.FileName);
        WpfMessageService.ShowInfo("XLSX успешно сохранен.");
    }

    private void OnPrint(object sender, RoutedEventArgs e)
    {
        if (_currentData == null || _currentData.Rows.Count == 0)
        {
            WpfMessageService.ShowInfo("Печать недоступна: сначала загрузите отчёт с данными.");
            return;
        }

        var printDialog = new System.Windows.Controls.PrintDialog();
        if (printDialog.ShowDialog() != true)
            return;

        var doc = BuildPrintDocument(_currentData, ReportsCombo.SelectedItem?.ToString() ?? "Отчет");
        printDialog.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator, "Baltika report");
    }

    /// <summary>Формирует <see cref="FlowDocument"/> с таблицей для передачи в <see cref="System.Windows.Controls.PrintDialog"/>.</summary>
    private static FlowDocument BuildPrintDocument(DataTable data, string title)
    {
        var doc = new FlowDocument
        {
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 10,
            PagePadding = new Thickness(30)
        };
        doc.Blocks.Add(new Paragraph(new Run(title)) { FontSize = 14, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 12) });

        var table = new Table();
        foreach (DataColumn _ in data.Columns)
            table.Columns.Add(new TableColumn());

        var rg = new TableRowGroup();
        var header = new TableRow();
        foreach (DataColumn col in data.Columns)
            header.Cells.Add(new TableCell(new Paragraph(new Run(col.ColumnName))) { FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0.5), Padding = new Thickness(4) });
        rg.Rows.Add(header);

        foreach (DataRow row in data.Rows)
        {
            var tr = new TableRow();
            foreach (DataColumn col in data.Columns)
                tr.Cells.Add(new TableCell(new Paragraph(new Run(Convert.ToString(row[col]) ?? string.Empty))) { BorderThickness = new Thickness(0.5), Padding = new Thickness(4) });
            rg.Rows.Add(tr);
        }

        table.RowGroups.Add(rg);
        doc.Blocks.Add(table);
        return doc;
    }

    private static string[] GetHeaderValues(DataTable dt)
    {
        var headers = new string[dt.Columns.Count];
        for (int i = 0; i < dt.Columns.Count; i++)
            headers[i] = EscapeCsv(dt.Columns[i].ColumnName);
        return headers;
    }

    private static string[] GetRowValues(DataRow row)
    {
        var values = new string[row.Table.Columns.Count];
        for (int i = 0; i < row.Table.Columns.Count; i++)
            values[i] = EscapeCsv(row[i]?.ToString() ?? string.Empty);
        return values;
    }

    /// <summary>Экранирует значение для CSV (точка с запятой, кавычки, перенос строки).</summary>
    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(';') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }

    /// <summary>Формирует имя файла по умолчанию для экспорта на основе выбранного отчёта и текущей даты.</summary>
    private string BuildDefaultExportName(string extension)
    {
        var name = (ReportsCombo.SelectedItem?.ToString() ?? "report")
            .Replace(":", "_")
            .Replace("/", "_")
            .Replace("\\", "_");
        return $"{name}_{DateTime.Now:yyyyMMdd_HHmm}.{extension}";
    }

}
