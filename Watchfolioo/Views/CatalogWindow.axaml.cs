namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Linq;
using Watchfolioo.Models;

public partial class CatalogWindow : Window
{
    private List<Series> _allMovies;
    private string _activeCategory = "Усі";

    private readonly List<Category> _categories = new()
    {
        new Category { Name = "Усі",     Tag = "Усі" },
        new Category { Name = "Екшн",    Tag = "Екшн" },
        new Category { Name = "Драма",   Tag = "Драма" },
        new Category { Name = "Жахи",    Tag = "Жахи" },
        new Category { Name = "Комедія", Tag = "Комедія" },
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

        BuildCategories();
        NavigateTo(new HomePage());
        SetActiveNav(HomeBtn);
    }

    public void NavigateToPage(UserControl page)
    {
        PageFrame.Content = page;
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
        CategoryPanel.Children.Clear();
        foreach (var cat in _categories)
        {
            var btn = new Button { Content = cat.Name, Tag = cat.Tag };
            btn.Classes.Add("CategoryTag");
            if (cat.Tag == _activeCategory)
                btn.Classes.Add("Active");
            btn.Click += Category_OnClick;
            CategoryPanel.Children.Add(btn);
        }
    }

    private void SetActiveNav(Button active)
    {
        var buttons = new[] { HomeBtn, MoviesBtn, SeriesBtn, FavBtn, SettingsBtn };
        foreach (var btn in buttons)
            foreach (var child in ((StackPanel)btn.Content!).Children.OfType<TextBlock>())
                child.Foreground = Brush.Parse("#888888");

        foreach (var child in ((StackPanel)active.Content!).Children.OfType<TextBlock>())
            child.Foreground = Brush.Parse("#E50914");
    }

    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(text))
        {
            NavigateTo(new HomePage());
            SetActiveNav(HomeBtn);
        }
        else
        {
            var results = _allMovies
                .Where(m => m.Title.ToLower().Contains(text.ToLower()))
                .ToList();
            var listPage = CreateListPage();
            listPage.LoadMovies(results);
            NavigateTo(listPage);
        }
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
        NavigateTo(new HomePage());
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
        listPage.LoadMovies(_allMovies.Where(m => m.IsFavorite).ToList(), "Обране");
        NavigateTo(listPage);
    }

    private void Nav_Settings(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(SettingsBtn);
        NavigateTo(new SettingsPage("користувач"));
    }
}