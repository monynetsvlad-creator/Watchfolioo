namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections.Generic;

public partial class SettingsPage : UserControl
{
    private bool _isDark = true;
    private readonly HashSet<string> _selectedGenres = new();

    public SettingsPage(string param = "")
    {
        InitializeComponent();
        if (!string.IsNullOrEmpty(param))
            ParamText.Text = $"Параметр: {param}";
        
        SetupGenreClicks();
    }

    private void SetupGenreClicks()
    {
        foreach (var child in GenrePanel.Children)
        {
            if (child is Border border)
            {
                border.PointerPressed += (s, e) =>
                {
                    if (border.Child is TextBlock text)
                    {
                        var genre = text.Text ?? "";
                        if (_selectedGenres.Contains(genre))
                        {
                           
                            _selectedGenres.Remove(genre);
                            border.Background = Brush.Parse("#222222");
                        }
                        else
                        {
                          
                            _selectedGenres.Add(genre);
                            border.Background = Brush.Parse("#E50914");
                        }
                    }
                };

                
                border.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
            }
        }
    }

    private void ThemeToggle_OnClick(object? sender, RoutedEventArgs e)
    {
        _isDark = !_isDark;

        var window = TopLevel.GetTopLevel(this) as Window;

        if (_isDark)
        {
            if (window != null)
                window.Background = Brush.Parse("#0F0F0F");
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
            ThemeLabel.Text = "Зараз: Темна";
        }
        else
        {
            if (window != null)
                window.Background = Brush.Parse("#F5F5F5");
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
            ThemeLabel.Text = "Зараз: Світла";
        }
    }
}