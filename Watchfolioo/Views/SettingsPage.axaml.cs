namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using Watchfolioo.Localization;

public partial class SettingsPage : UserControl
{
    private bool _isDark = true;
    private readonly HashSet<string> _selectedGenres = new();

    public SettingsPage(string param = "")
    {
        InitializeComponent();
        
        Strings.LanguageChanged += ApplyLocalization;
        
        ApplyLocalization();
        SetupGenreClicks();
        UpdateActiveLangButton();
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
            if (window != null) window.Background = Brush.Parse("#0F0F0F");
            Application.Current!.RequestedThemeVariant = ThemeVariant.Dark;
        }
        else
        {
            if (window != null) window.Background = Brush.Parse("#F5F5F5");
            Application.Current!.RequestedThemeVariant = ThemeVariant.Light;
        }
        
        ApplyLocalization();
    }

    private void Lang_UA(object? sender, RoutedEventArgs e) => SetLanguage("uk");
    private void Lang_EN(object? sender, RoutedEventArgs e) => SetLanguage("en");
    private void Lang_PL(object? sender, RoutedEventArgs e) => SetLanguage("pl");
    private void Lang_DE(object? sender, RoutedEventArgs e) => SetLanguage("de");
    private void Lang_FR(object? sender, RoutedEventArgs e) => SetLanguage("fr");
    private void Lang_ES(object? sender, RoutedEventArgs e) => SetLanguage("es");

    private void SetLanguage(string lang)
    {
        Strings.SetLanguage(lang);
    }

    private void ApplyLocalization()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (SettingsTitle != null) SettingsTitle.Text = Strings.Get("settings_title");
            if (GenresTitle != null)   GenresTitle.Text   = Strings.Get("genres_title");
            if (ThemeTitle != null)    ThemeTitle.Text    = Strings.Get("theme");
            if (LanguageTitle != null) LanguageTitle.Text = Strings.Get("language");
            
            if (ThemeLabel != null)
            {
                ThemeLabel.Text = _isDark
                    ? Strings.Get("theme_dark")
                    : Strings.Get("theme_light");
            }
            
            if (ThemeToggleBtn != null) ThemeToggleBtn.Content = Strings.Get("change");

            UpdateActiveLangButton();
            TranslateGenresInSettings();
        });
    }

    private void TranslateGenresInSettings()
    {
        var genreKeys = new Dictionary<string, string>
        {
            { "Усі", "genre_all" }, { "All", "genre_all" },
            { "Екшн", "genre_action" }, { "Action", "genre_action" },
            { "Драма", "genre_drama" }, { "Drama", "genre_drama" },
            { "Жахи", "genre_horror" }, { "Horror", "genre_horror" },
            { "Комедія", "genre_comedy" }, { "Comedy", "genre_comedy" },
            { "Фантастика", "genre_scifi" }, { "Sci-Fi", "genre_scifi" },
            { "Мультфільми", "genre_animation" }, { "Animation", "genre_animation" },
            { "Трилери", "genre_thriller" }, { "Thriller", "genre_thriller" }
        };

        if (GenrePanel != null)
        {
            foreach (var child in GenrePanel.Children)
            {
                if (child is Border border && border.Child is TextBlock text)
                {
                    foreach (var entry in genreKeys)
                    {
                        if (text.Text == entry.Key || text.Text == Strings.Get(entry.Value))
                        {
                            text.Text = Strings.Get(entry.Value);
                            break;
                        }
                    }
                }
            }
        }
    }

    private void UpdateActiveLangButton()
    {
        var langButtons = new[]
        {
            (LangUA, "uk"),
            (LangEN, "en"),
            (LangPL, "pl"),
            (LangDE, "de"),
            (LangFR, "fr"),
            (LangES, "es"),
        };

        foreach (var (btn, code) in langButtons)
        {
            if (btn != null)
            {
                btn.Background = code == Strings.CurrentLanguage
                    ? Brush.Parse("#E50914")
                    : Brush.Parse("#222222");
            }
        }
    }
}