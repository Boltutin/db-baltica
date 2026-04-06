using System;
using System.Collections.Generic;
using System.Data;

namespace BaltikaApp.Wpf.Helpers;

/// <summary>
/// Утилиты для работы с <see cref="System.Windows.Controls.DataGrid"/>:
/// извлечение ID выбранной строки, отображение деталей записи.
/// </summary>
internal static class DataGridHelper
{
    /// <summary>
    /// Переименовывает проблемные для WPF имена колонок (например, с точкой),
    /// чтобы автогенерация привязок DataGrid не интерпретировала имя как путь свойства.
    /// </summary>
    public static void NormalizeColumnNamesForWpf(DataTable table)
    {
        foreach (DataColumn col in table.Columns)
        {
            if (string.IsNullOrWhiteSpace(col.ColumnName)) continue;
            if (!col.ColumnName.Contains('.')) continue;

            var safe = col.ColumnName.Replace(".", "");
            while (table.Columns.Contains(safe))
                safe += "_";
            col.ColumnName = safe;
        }
    }

    /// <summary>
    /// Возвращает целочисленный ID из выбранной строки <see cref="System.Windows.Controls.DataGrid"/>,
    /// или <c>null</c>, если ничего не выбрано или столбец отсутствует.
    /// </summary>
    public static int? GetSelectedId(object? selectedItem, string idColumn)
    {
        if (selectedItem is not DataRowView row)
            return null;
        return row.Row.Table.Columns.Contains(idColumn)
            ? Convert.ToInt32(row.Row[idColumn])
            : null;
    }

    /// <summary>
    /// Показывает все колонки выбранной строки в информационном диалоге.
    /// </summary>
    public static void ShowRowDetails(object? selectedItem, string title)
    {
        if (selectedItem is not DataRowView rowView) return;
        var lines = new List<string>();
        foreach (DataColumn col in rowView.Row.Table.Columns)
            lines.Add($"{col.ColumnName}: {Convert.ToString(rowView.Row[col])}");
        WpfMessageService.ShowInfo(string.Join(Environment.NewLine, lines), title);
    }
}
