namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Styling;

public partial class SettingsPage : UserControl
{
    private bool _isDark = true;

    public SettingsPage(string param = "")
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(param))
            ParamText.Text = $"Параметр: {param}";
    }

    private void ThemeToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = _isDark
                ? ThemeVariant.Dark
                : ThemeVariant.Light;
        }

        ThemeLabel.Text = _isDark ? "Зараз: Темна" : "Зараз: Світла";
    }
}