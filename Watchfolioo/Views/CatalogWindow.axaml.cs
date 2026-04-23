namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Watchfolioo.Models;
using Watchfolioo.Services;
using Watchfolioo.Localization;

public partial class CatalogWindow : Window
{
    private List<Series> _allMovies;
    private string _activeCategory = "Усі";
    private HomePage? _homePage = new();
    private readonly TranslateService _translator = new();
    private readonly TmdbService _tmdb = new();
    
    private UserControl? _currentPage;

    public CatalogWindow()
    {
        InitializeComponent();

        _allMovies = new List<Series>
        {
            new Series { Title = "Дюна: Частина 2",  Genre = "Екшн",    Type = "Фільм",  Rating = "8.3", ImdbId = "823464" },
            new Series { Title = "Оппенгаймер",       Genre = "Драма",   Type = "Фільм",  Rating = "8.1", ImdbId = "872585" },
            new Series { Title = "Інтерстеллар",       Genre = "Екшн",    Type = "Фільм",  Rating = "8.4", ImdbId = "157336" },
            new Series { Title = "Відьмак",            Genre = "Екшн",    Type = "Серіал", Rating = "8.1", ImdbId = "71912"  }
        };

        Strings.LanguageChanged += ApplyLocalization;
        Strings.LanguageChanged += async () => await TranslateCatalogGenresAsync();

        BuildCategories();
        GoHome();
        SetActiveNav(HomeBtn);
        ApplyLocalization();
    }

    public void NavigateToPage(UserControl page) => NavigateTo(page);

    private void NavigateTo(UserControl? page)
    {
        if (page == null) return;
        _currentPage = page;
        PageFrame.Content = page;
    }

    private string GetSearchTermForGenre(string genreTag)
    {
        return genreTag switch
        {
            "Екшн" => "Action",
            "Драма" => "Drama",
            "Жахи" => "Horror",
            "Комедія" => "Comedy",
            "Фантастика" => "Sci-Fi",
            "Мультфільми" => "Animation",
            "Трилери" => "Thriller",
            _ => genreTag
        };
    }

    private async Task TranslateCatalogGenresAsync()
    {
        var lang = Strings.CurrentLanguage;

        foreach (var movie in _allMovies)
        {
            if (!string.IsNullOrEmpty(movie.Title))
                movie.Title = await _translator.TranslateAsync(movie.Title, lang);
            if (!string.IsNullOrEmpty(movie.Genre))
                movie.Genre = await _translator.TranslateAsync(movie.Genre, lang);
        }
    }

    private void GoHome() => NavigateTo(_homePage);

    private void ApplyLocalization()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (AppTitle != null) AppTitle.Text = Strings.Get("app_title");
            if (SearchBox != null) SearchBox.PlaceholderText = Strings.Get("search_hint");
            
            UpdateNavText(HomeBtn,     "nav_home");
            UpdateNavText(MoviesBtn,   "nav_movies");
            UpdateNavText(SeriesBtn,   "nav_series");
            UpdateNavText(FavBtn,      "nav_favorites");
            UpdateNavText(SettingsBtn, "nav_settings");

            BuildCategories();

            if (_currentPage is SettingsPage) NavigateTo(new SettingsPage("користувач"));
            else if (_currentPage is HomePage) GoHome();
        });
    }

    private void UpdateNavText(Button btn, string key)
    {
        if (btn?.Content is StackPanel sp)
        {
            var texts = sp.Children.OfType<TextBlock>().ToList();
            if (texts.Count >= 2) texts[1].Text = Strings.Get(key);
        }
    }

    public void AddMoviesToCatalog(List<Series> movies)
    {
        foreach (var movie in movies)
        {
            if (!_allMovies.Any(m => m.ImdbId == movie.ImdbId))
                _allMovies.Add(movie);
        }
    }

    private ListPage CreateListPage()
    {
        var listPage = new ListPage();
        listPage.SetCatalogWindow(this);
        return listPage;
    }

    private void BuildCategories()
    {
        if (CategoryPanel == null) return;
        CategoryPanel.Children.Clear();
        
        var cats = new[]
        {
            ("genre_all",       "Усі"),
            ("genre_action",    "Екшн"),
            ("genre_drama",     "Драма"),
            ("genre_horror",    "Жахи"),
            ("genre_comedy",    "Комедія"),
            ("genre_scifi",     "Фантастика"),
            ("genre_animation", "Мультфільми"),
            ("genre_thriller",  "Трилери")
        };

        foreach (var (key, tag) in cats)
        {
            var btn = new Button { Content = Strings.Get(key), Tag = tag };
            btn.Classes.Add("CategoryTag");
            if (tag == _activeCategory) btn.Classes.Add("Active");
            btn.Click += Category_OnClick;
            CategoryPanel.Children.Add(btn);
        }
    }

    private void SetActiveNav(Button active)
    {
        var buttons = new[] { HomeBtn, MoviesBtn, SeriesBtn, FavBtn, SettingsBtn };
        foreach (var btn in buttons)
            if (btn?.Content is StackPanel sp)
                foreach (var child in sp.Children.OfType<TextBlock>())
                    child.Foreground = Brush.Parse("#888888");

        if (active?.Content is StackPanel activeSp)
            foreach (var child in activeSp.Children.OfType<TextBlock>())
                child.Foreground = Brush.Parse("#E50914");
    }

    private async void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            GoHome();
            SetActiveNav(HomeBtn);
            return;
        }

        var lang = Strings.CurrentLanguage;
        var apiResults = await _tmdb.SearchMovies(text);

        foreach (var apiMovie in apiResults)
        {
            if (!_allMovies.Any(m => m.ImdbId == apiMovie.ImdbId))
            {
                if (!string.IsNullOrEmpty(apiMovie.Title))
                    apiMovie.Title = await _translator.TranslateAsync(apiMovie.Title, lang);
                if (!string.IsNullOrEmpty(apiMovie.Genre))
                    apiMovie.Genre = await _translator.TranslateAsync(apiMovie.Genre, lang);
                
                _allMovies.Add(apiMovie);
            }
        }

        var localResults = _allMovies
            .Where(m => m.Title != null && m.Title.ToLower().Contains(text.ToLower()))
            .ToList();

        var listPage = CreateListPage();
        listPage.LoadMovies(localResults);
        NavigateTo(listPage);
    }

    private async void Category_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            _activeCategory = tag;
            BuildCategories();

            var lang = Strings.CurrentLanguage;
            List<Series> resultsToDisplay;

            if (tag == "Усі")
            {
                resultsToDisplay = _allMovies.ToList();
            }
            else
            {
                resultsToDisplay = _allMovies.Where(m => m.Genre == tag).ToList();

                if (resultsToDisplay.Count < 5)
                {
                    var searchTerm = GetSearchTermForGenre(tag);
                    var apiMovies = await _tmdb.SearchMovies(searchTerm);

                    foreach (var movie in apiMovies)
                    {
                        if (!_allMovies.Any(m => m.ImdbId == movie.ImdbId))
                        {
                            movie.Genre = tag;
                            if (!string.IsNullOrEmpty(movie.Title))
                                movie.Title = await _translator.TranslateAsync(movie.Title, lang);
                                
                            _allMovies.Add(movie);
                            resultsToDisplay.Add(movie);
                        }
                        if (resultsToDisplay.Count >= 12) break;
                    }
                }
            }

            var listPage = CreateListPage();
            listPage.LoadMovies(resultsToDisplay);
            NavigateTo(listPage);
        }
    }

    private void Nav_Home(object? sender, RoutedEventArgs e) { SetActiveNav(HomeBtn); GoHome(); }
    private void Nav_Movies(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(MoviesBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.Type == "Фільм" || m.Type == "Movie").ToList());
        NavigateTo(listPage);
    }
    private void Nav_Series(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(SeriesBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.Type == "Серіал" || m.Type == "Series").ToList());
        NavigateTo(listPage);
    }
    private void Nav_Fav(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(FavBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.IsFavorite).ToList(), Strings.Get("favorites"));
        NavigateTo(listPage);
    }
    private void Nav_Settings(object? sender, RoutedEventArgs e) { SetActiveNav(SettingsBtn); NavigateTo(new SettingsPage("користувач")); }
}