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
    private HomePage? _homePage;
    private readonly TranslateService _translator = new();
    
    private readonly List<Category> _categories = new()
    {
        new Category { Name = "Усі",         Tag = "Усі" },
        new Category { Name = "Екшн",        Tag = "Екшн" },
        new Category { Name = "Драма",       Tag = "Драма" },
        new Category { Name = "Жахи",        Tag = "Жахи" },
        new Category { Name = "Комедія",     Tag = "Комедія" },
        new Category { Name = "Фантастика",  Tag = "Фантастика" },
        new Category { Name = "Мультфільми", Tag = "Мультфільми" },
        new Category { Name = "Трилери",     Tag = "Трилери" }
    };

    private readonly List<string> _cardColors = new()
    {
        "#1a1a2e", "#16213e", "#2d1b69", "#1d4350",
        "#b92b27", "#093028", "#232526", "#614385"
    };

    private UserControl? _currentPage;

    public CatalogWindow()
    {
        InitializeComponent();

        _allMovies = new List<Series>
        {
            new Series { Title = "Дюна: Частина 2",  Genre = "Екшн",    Type = "Фільм",  Rating = "8.7", ImdbId = "tt15239678" },
            new Series { Title = "Оппенгаймер",       Genre = "Драма",   Type = "Фільм",  Rating = "8.4", ImdbId = "tt15398776" },
            new Series { Title = "Джокер",             Genre = "Драма",   Type = "Фільм",  Rating = "8.5", ImdbId = "tt7286456"  },
            new Series { Title = "Інтерстеллар",       Genre = "Екшн",    Type = "Фільм",  Rating = "8.6", ImdbId = "tt0816692"  },
            new Series { Title = "Гра в кальмара 2",  Genre = "Жахи",    Type = "Серіал", Rating = "9.1", ImdbId = "tt21209876" },
            new Series { Title = "Відьмак",            Genre = "Екшн",    Type = "Серіал", Rating = "8.2", ImdbId = "tt5180504"  },
            new Series { Title = "Хаус Дракона",       Genre = "Драма",   Type = "Серіал", Rating = "8.6", ImdbId = "tt11198330" },
            new Series { Title = "Чорне дзеркало",     Genre = "Жахи",    Type = "Серіал", Rating = "9.3", ImdbId = "tt2085059"  },
            new Series { Title = "Форс мажор",         Genre = "Комедія", Type = "Серіал", Rating = "8.1", ImdbId = "tt1632701"  },
            new Series { Title = "Той, хто вижив",     Genre = "Екшн",    Type = "Серіал", Rating = "8.9", ImdbId = "tt2741602"  },
        };

        Strings.LanguageChanged += ApplyLocalization;
        Strings.LanguageChanged += async () => await TranslateCatalogGenresAsync();

        BuildCategories();
        GoHome();
        SetActiveNav(HomeBtn);
        ApplyLocalization();
    }

    private async Task TranslateCatalogGenresAsync()
    {
        var lang = Strings.CurrentLanguage;
        if (lang == "en") return;

        foreach (var movie in _allMovies)
        {
            if (!string.IsNullOrEmpty(movie.Genre))
                movie.Genre = await _translator.TranslateAsync(movie.Genre, lang);
        }
    }

    private void GoHome()
    {
        _homePage = new HomePage();
        NavigateTo(_homePage);
    }

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

            if (_currentPage != null)
            {
                if (_currentPage is SettingsPage)
                    NavigateTo(new SettingsPage("користувач"));
                else if (_currentPage is HomePage)
                    GoHome();
            }
        });
    }

    private void UpdateNavText(Button btn, string key)
    {
        if (btn != null && btn.Content is StackPanel sp)
        {
            var texts = sp.Children.OfType<TextBlock>().ToList();
            if (texts.Count >= 2)
                texts[1].Text = Strings.Get(key);
        }
    }

    public void NavigateToPage(UserControl page)
    {
        PageFrame.Content = page;
    }

    public void AddMoviesToCatalog(List<Series> movies)
    {
        foreach (var movie in movies)
        {
            bool exists = _allMovies.Any(m => m.Title?.ToLower() == movie.Title?.ToLower());
            if (!exists) _allMovies.Add(movie);
        }
    }

    private void NavigateTo(UserControl page)
    {
        _currentPage = page;
        PageFrame.Content = page;
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
            if (tag == _activeCategory)
                btn.Classes.Add("Active");
            btn.Click += Category_OnClick;
            CategoryPanel.Children.Add(btn);
        }
    }

    private void SetActiveNav(Button active)
    {
        var buttons = new[] { HomeBtn, MoviesBtn, SeriesBtn, FavBtn, SettingsBtn };
        foreach (var btn in buttons)
            if (btn != null && btn.Content is StackPanel sp)
                foreach (var child in sp.Children.OfType<TextBlock>())
                    child.Foreground = Brush.Parse("#888888");

        if (active != null && active.Content is StackPanel activeSp)
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

        var localResults = _allMovies
            .Where(m => m.Title != null && m.Title.ToLower().Contains(text.ToLower()))
            .ToList();

        var omdb = new OmdbService();
        var omdbResults = await omdb.SearchMovies(text);

        foreach (var omdbMovie in omdbResults)
        {
            bool exists = _allMovies.Any(m => m.Title?.ToLower() == omdbMovie.Title?.ToLower());
            if (!exists)
            {
                omdbMovie.Type = "Фільм";
                _allMovies.Add(omdbMovie);
                localResults.Add(omdbMovie);
            }
        }

        var listPage = CreateListPage();
        listPage.LoadMovies(localResults);
        NavigateTo(listPage);
    }

    private void Category_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            _activeCategory = tag;
            BuildCategories();
            var filtered = tag == "Усі"
                ? _allMovies
                : _allMovies.Where(m => m.Genre == tag).ToList();
            var listPage = CreateListPage();
            listPage.LoadMovies(filtered);
            NavigateTo(listPage);
        }
    }

    private void Nav_Home(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(HomeBtn);
        GoHome();
    }

    private void Nav_Movies(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(MoviesBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.Type == "Фільм").ToList());
        NavigateTo(listPage);
    }

    private void Nav_Series(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(SeriesBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.Type == "Серіал").ToList());
        NavigateTo(listPage);
    }

    private void Nav_Fav(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(FavBtn);
        var listPage = CreateListPage();
        listPage.LoadMovies(_allMovies.Where(m => m.IsFavorite).ToList(), Strings.Get("favorites"));
        NavigateTo(listPage);
    }

    private void Nav_Settings(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(SettingsBtn);
        NavigateTo(new SettingsPage("користувач"));
    }
}