namespace Watchfolio.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Linq;
using Watchfolio.Models;

public partial class CatalogWindow : Window
{
    private List<Series> _allMovies;
    private string _activeCategory = "Усі";
    private string _activeTab = "Головна";

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

    public CatalogWindow()
    {
        InitializeComponent();
        _allMovies = new List<Series>
        {
            new Series { Title = "Дюна: Частина 2",  Genre = "Екшн",    Type = "Фільм",  Rating = "8.7" },
            new Series { Title = "Оппенгаймер",       Genre = "Драма",   Type = "Фільм",  Rating = "8.4" },
            new Series { Title = "Джокер",             Genre = "Драма",   Type = "Фільм",  Rating = "8.5" },
            new Series { Title = "Інтерстеллар",       Genre = "Екшн",    Type = "Фільм",  Rating = "8.6" },
            new Series { Title = "Гра в кальмара 2",  Genre = "Жахи",    Type = "Серіал", Rating = "9.1" },
            new Series { Title = "Відьмак",            Genre = "Екшн",    Type = "Серіал", Rating = "8.2" },
            new Series { Title = "Хаус Дракона",       Genre = "Драма",   Type = "Серіал", Rating = "8.6" },
            new Series { Title = "Чорне дзеркало",     Genre = "Жахи",    Type = "Серіал", Rating = "9.3" },
            new Series { Title = "Форс мажор",         Genre = "Комедія", Type = "Серіал", Rating = "8.1" },
            new Series { Title = "Той, хто вижив",     Genre = "Екшн",    Type = "Серіал", Rating = "8.9" },
        };

        BuildCategories();
        RenderAllSections();
        SetActiveNav(HomeBtn);
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

    
    private void RenderAllSections()
    {
        SearchSection.IsVisible  = false;
        PopularSection.IsVisible = true;
        MoviesSection.IsVisible  = true;
        SeriesSection.IsVisible  = true;

        var filtered = GetFilteredByCategory(_allMovies);

        FillRow(PopularRow,  filtered.Take(6).ToList());
        FillRow(MoviesRow,   filtered.Where(m => m.Type == "Фільм").ToList());
        FillRow(SeriesRow,   filtered.Where(m => m.Type == "Серіал").ToList());
    }

    
    private void RenderSearchResults(string query)
    {
        PopularSection.IsVisible = false;
        MoviesSection.IsVisible  = false;
        SeriesSection.IsVisible  = false;
        SearchSection.IsVisible  = true;

        var results = _allMovies
            .Where(m => m.Title.ToLower().Contains(query.ToLower()))
            .ToList();

        SearchResultsTitle.Text = $"Результати пошуку: {results.Count}";
        SearchRow.Children.Clear();

        if (results.Count == 0)
        {
            SearchRow.Children.Add(new TextBlock
            {
                Text = "Нічого не знайдено",
                Foreground = Brushes.Gray,
                FontSize = 15,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            });
            return;
        }

        foreach (var movie in results)
            SearchRow.Children.Add(BuildListCard(movie));
    }

    
    private void FillRow(StackPanel row, List<Series> movies)
    {
        row.Children.Clear();
        int colorIndex = 0;
        foreach (var movie in movies)
        {
            row.Children.Add(BuildPosterCard(movie, _cardColors[colorIndex % _cardColors.Count]));
            colorIndex++;
        }
    }

    
    private Border BuildPosterCard(Series movie, string bgColor)
    {
        var ratingBlock = new TextBlock
        {
            Text = $"★ {movie.Rating}",
            Foreground = Brush.Parse("#FFD700"),
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Avalonia.Thickness(6)
        };

        var poster = new Border
        {
            Width = 110,
            Height = 155,
            CornerRadius = new Avalonia.CornerRadius(8),
            Background = Brush.Parse(bgColor),
            Child = ratingBlock
        };

        var titleBlock = new TextBlock
        {
            Text = movie.Title,
            Foreground = Brush.Parse("#CCCCCC"),
            FontSize = 11,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            MaxWidth = 110,
            Margin = new Avalonia.Thickness(2, 4, 2, 0)
        };

        var badge = new Border
        {
            Background = Brush.Parse("#E50914"),
            CornerRadius = new Avalonia.CornerRadius(3),
            Padding = new Avalonia.Thickness(5, 2),
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Avalonia.Thickness(2, 2, 0, 0),
            Child = new TextBlock
            {
                Text = movie.Type,
                Foreground = Brushes.White,
                FontSize = 9
            }
        };

        var favBtn = new Button
        {
            Content = movie.IsFavorite ? "❤️" : "🤍",
            Background = Brushes.Transparent,
            FontSize = 14,
            Padding = new Avalonia.Thickness(2),
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Avalonia.Thickness(0, 2, 0, 0)
        };
        favBtn.Click += (s, e) =>
        {
            movie.IsFavorite = !movie.IsFavorite;
            RenderAllSections();
        };

        var bottomRow = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("*,Auto")
        };
        Grid.SetColumn(badge, 0);
        Grid.SetColumn(favBtn, 1);
        bottomRow.Children.Add(badge);
        bottomRow.Children.Add(favBtn);

        var card = new StackPanel
        {
            Width = 110,
            Children = { poster, titleBlock, bottomRow }
        };

        return new Border { Child = card };
    }

    
    private Border BuildListCard(Series movie)
    {
        var title = new TextBlock
        {
            Text = movie.Title,
            Foreground = Brushes.White,
            FontSize = 15,
            FontWeight = FontWeight.Medium
        };

        var typeBadge = new Border
        {
            Background = Brush.Parse("#E50914"),
            CornerRadius = new Avalonia.CornerRadius(4),
            Padding = new Avalonia.Thickness(6, 2),
            Child = new TextBlock { Text = movie.Type, Foreground = Brushes.White, FontSize = 11 }
        };

        var rating = new TextBlock
        {
            Text = $"★ {movie.Rating}",
            Foreground = Brush.Parse("#FFD700"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(8, 0, 0, 0)
        };

        var genre = new TextBlock
        {
            Text = movie.Genre,
            Foreground = Brush.Parse("#888888"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Avalonia.Thickness(8, 0, 0, 0)
        };

        var favBtn = new Button
        {
            Content = movie.IsFavorite ? "❤️" : "🤍",
            Background = Brushes.Transparent,
            FontSize = 18,
            Padding = new Avalonia.Thickness(4)
        };
        favBtn.Click += (s, e) =>
        {
            movie.IsFavorite = !movie.IsFavorite;
            RenderSearchResults(SearchBox.Text ?? "");
        };

        var infoRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Avalonia.Thickness(0, 6, 0, 0),
            Children = { typeBadge, rating, genre }
        };

        var textStack = new StackPanel { Children = { title, infoRow } };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        Grid.SetColumn(textStack, 0);
        Grid.SetColumn(favBtn, 1);
        grid.Children.Add(textStack);
        grid.Children.Add(favBtn);

        return new Border
        {
            Background = Brush.Parse("#1E1E1E"),
            CornerRadius = new Avalonia.CornerRadius(10),
            Padding = new Avalonia.Thickness(14),
            Margin = new Avalonia.Thickness(0, 0, 0, 8),
            Child = grid
        };
    }

    
    private IEnumerable<Series> GetFilteredByCategory(IEnumerable<Series> source)
    {
        if (_activeCategory == "Усі") return source;
        return source.Where(m => m.Genre == _activeCategory);
    }

    
    private void SetActiveNav(Button active)
    {
        var buttons = new[] { HomeBtn, MoviesBtn, SeriesBtn, FavBtn, SettingsBtn };
        foreach (var btn in buttons)
        {
            btn.Classes.Remove("Active");
            
            foreach (var child in ((StackPanel)btn.Content!).Children.OfType<TextBlock>())
                child.Foreground = Brush.Parse("#888888");
        }
        foreach (var child in ((StackPanel)active.Content!).Children.OfType<TextBlock>())
            child.Foreground = Brush.Parse("#E50914");
    }

    
    private void SearchBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        var text = SearchBox.Text ?? "";
        if (string.IsNullOrWhiteSpace(text))
            RenderAllSections();
        else
            RenderSearchResults(text);
    }

    private void Category_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string tag)
        {
            _activeCategory = tag;
            BuildCategories();
            RenderAllSections();
        }
    }

    private void Nav_Home(object? sender, RoutedEventArgs e)
    {
        _activeTab = "Головна";
        SetActiveNav(HomeBtn);
        RenderAllSections();
    }

    private void Nav_Movies(object? sender, RoutedEventArgs e)
    {
        _activeTab = "Фільми";
        SetActiveNav(MoviesBtn);
        SearchSection.IsVisible  = false;
        PopularSection.IsVisible = false;
        SeriesSection.IsVisible  = false;
        MoviesSection.IsVisible  = true;
        FillRow(MoviesRow, GetFilteredByCategory(_allMovies.Where(m => m.Type == "Фільм")).ToList());
    }

    private void Nav_Series(object? sender, RoutedEventArgs e)
    {
        _activeTab = "Серіали";
        SetActiveNav(SeriesBtn);
        SearchSection.IsVisible  = false;
        PopularSection.IsVisible = false;
        MoviesSection.IsVisible  = false;
        SeriesSection.IsVisible  = true;
        FillRow(SeriesRow, GetFilteredByCategory(_allMovies.Where(m => m.Type == "Серіал")).ToList());
    }

    private void Nav_Fav(object? sender, RoutedEventArgs e)
    {
        _activeTab = "Обране";
        SetActiveNav(FavBtn);
        SearchSection.IsVisible  = true;
        PopularSection.IsVisible = false;
        MoviesSection.IsVisible  = false;
        SeriesSection.IsVisible  = false;
        SearchResultsTitle.Text  = "Обране";
        SearchRow.Children.Clear();
        var favs = _allMovies.Where(m => m.IsFavorite).ToList();
        if (favs.Count == 0)
        {
            SearchRow.Children.Add(new TextBlock
            {
                Text = "Ще нічого не додано до обраного",
                Foreground = Brushes.Gray,
                FontSize = 15,
                Margin = new Avalonia.Thickness(0, 20, 0, 0)
            });
        }
        else
        {
            foreach (var movie in favs)
                SearchRow.Children.Add(BuildListCard(movie));
        }
    }

    private void Nav_Settings(object? sender, RoutedEventArgs e)
    {
        SetActiveNav(SettingsBtn);
        
    }
}