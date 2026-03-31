using System.Windows;

namespace BaltikaApp.Wpf;

/// <summary>
/// Диалог ввода пароля для переключения приложения в режим редактирования.
/// Поддерживает показ/скрытие пароля через чекбокс.
/// </summary>
public partial class PasswordPromptDialog : Window
{
    /// <summary>Введённый пользователем пароль (из скрытого или видимого поля ввода).</summary>
    public string Password => ShowCheck.IsChecked == true ? VisiblePasswordBox.Text : PasswordBox.Password;

    public PasswordPromptDialog()
    {
        InitializeComponent();
    }

    private void OnShowPassword(object sender, RoutedEventArgs e)
    {
        if (ShowCheck.IsChecked == true)
        {
            VisiblePasswordBox.Text = PasswordBox.Password;
            VisiblePasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
            VisiblePasswordBox.Focus();
        }
        else
        {
            PasswordBox.Password = VisiblePasswordBox.Text;
            VisiblePasswordBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
            PasswordBox.Focus();
        }
    }

    private void OnVisiblePasswordChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (ShowCheck.IsChecked == true)
            PasswordBox.Password = VisiblePasswordBox.Text;
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Password))
        {
            WpfMessageService.ShowInfo("Введите пароль.", "Проверка");
            return;
        }

        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
