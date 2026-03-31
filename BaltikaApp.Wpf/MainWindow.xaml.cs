using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using BaltikaApp.Data;
using BaltikaApp.Wpf.Commands;

namespace BaltikaApp.Wpf;

/// <summary>
/// Главное окно WPF-приложения: навигация по разделам, KPI, переключение режимов доступа,
/// управление настройками подключения к PostgreSQL.
/// </summary>
public partial class MainWindow : Window
{
    private readonly AppState _state = new();

    public ICommand ShowShipsCommand          { get; }
    public ICommand ShowShipmentsCommand      { get; }
    public ICommand ShowCargoCommand          { get; }
    public ICommand ShowClientsCommand        { get; }
    public ICommand ShowReportsCommand        { get; }
    public ICommand ShowByPeriodCommand       { get; }
    public ICommand ShowReferencesCommand     { get; }
    public ICommand RefreshKpiCommand         { get; }
    public ICommand EnableWriterCommand       { get; }
    public ICommand DisableWriterCommand      { get; }
    public ICommand ConnectionSettingsCommand { get; }

    public MainWindow()
    {
        ShowShipsCommand          = new RelayCommand(() => OpenOrActivateWindow<ShipsWindow>());
        ShowShipmentsCommand      = new RelayCommand(() => OpenOrActivateWindow<ShipmentsWindow>());
        ShowCargoCommand          = new RelayCommand(() => OpenOrActivateWindow<CargoWindow>());
        ShowClientsCommand        = new RelayCommand(() => OpenOrActivateWindow<ClientsWindow>());
        ShowReportsCommand        = new RelayCommand(() => OpenOrActivateWindow<ReportsWindow>());
        ShowByPeriodCommand       = new RelayCommand(() => OpenOrActivateWindow<ShipmentsByPeriodWindow>());
        ShowReferencesCommand     = new RelayCommand(() => OpenOrActivateWindow<ReferenceDataWindow>());
        RefreshKpiCommand         = new RelayCommand(RefreshKpi);
        EnableWriterCommand       = new RelayCommand(EnableWriter, () => !ConnectionManager.IsWriter);
        DisableWriterCommand      = new RelayCommand(DisableWriter, () => ConnectionManager.IsWriter);
        ConnectionSettingsCommand = new RelayCommand(OpenConnectionSettings, () => ConnectionManager.IsWriter);

        InitializeComponent();
        DataContext = _state;
        RefreshKpi();
        Closed += (_, _) => _state.Dispose();
    }

    protected override void OnActivated(EventArgs e)
    {
        base.OnActivated(e);
        _state.RefreshConnectionState();
    }

    private void EnableWriter()
    {
        var pwd = new PasswordPromptDialog { Owner = this };
        if (pwd.ShowDialog() != true) return;
        if (ConnectionManager.TrySwitchToWriter(pwd.Password))
            WpfMessageService.ShowInfo(UiText.Main.WriterEnabled);
        else
            WpfMessageService.ShowInfo(UiText.Main.WriterPasswordInvalid);
        _state.RefreshConnectionState();
        RelayCommand.Refresh();
    }

    private void DisableWriter()
    {
        ConnectionManager.UseReader();
        _state.RefreshConnectionState();
        RelayCommand.Refresh();
        WpfMessageService.ShowInfo(UiText.Main.ReaderEnabled);
    }

    private void OpenConnectionSettings()
    {
        var dlg = new ConnectionSettingsWindow { Owner = this };
        if (dlg.ShowDialog() == true)
        {
            _state.RefreshConnectionState();
            RefreshKpi();
            WpfMessageService.ShowInfo(UiText.Main.ConnectionApplied);
        }
    }

    private void RefreshKpi()
    {
        try
        {
            if (!ValidationService.ValidateConnectionInput(DatabaseConnectionSettings.Host, DatabaseConnectionSettings.Database, out _))
                return;

            var dt = Db.Query(
                KpiQueries.MainDashboardStats,
                new Npgsql.NpgsqlParameter("@start", DateTime.Today.AddMonths(-1).Date),
                new Npgsql.NpgsqlParameter("@end", DateTime.Today.Date));

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                ShipmentsStatText.Text = Convert.ToString(row["shipments_last_month"]) ?? "0";
                CargoStatText.Text     = Convert.ToString(row["cargo_last_month"]) ?? "0";
            }

            _state.DbStatus = UiText.Main.DbAvailable;
            KpiUpdatedText.Text = $"{UiText.Main.KpiUpdatedPrefix}{DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex)
        {
            ShipmentsStatText.Text = "—";
            CargoStatText.Text     = "—";
            KpiUpdatedText.Text = UiText.Main.KpiError;
            _state.DbStatus = ex.Message.Contains("42703")
                ? UiText.Main.DbSchemaMismatch
                : UiText.Main.DbKpiGenericError;
        }
    }

    private readonly Dictionary<Type, Window> _openWindows = new();

    /// <summary>
    /// Открывает окно типа <typeparamref name="TWindow"/> или активирует уже открытое.
    /// Гарантирует, что одновременно существует не более одного экземпляра каждого типа окна.
    /// </summary>
    private void OpenOrActivateWindow<TWindow>() where TWindow : Window, new()
    {
        var key = typeof(TWindow);
        if (_openWindows.TryGetValue(key, out var existing))
        {
            if (existing.IsLoaded)
            {
                if (existing.WindowState == WindowState.Minimized)
                    existing.WindowState = WindowState.Normal;
                existing.Activate();
                return;
            }
        }

        var window = new TWindow { Owner = this };
        window.Closed += (_, _) => _openWindows.Remove(key);
        _openWindows[key] = window;
        window.Show();
    }
}