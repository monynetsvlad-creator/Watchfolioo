namespace Watchfolioo.Views;
using Avalonia.Controls;

public partial class SettingsPage : UserControl
{
    public SettingsPage(string param = "")
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(param))
            ParamText.Text = $"Параметр: {param}";
    }
}