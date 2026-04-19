namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Watchfolioo.Models;
using Watchfolioo.Services;

public partial class HomePage : UserControl
{
    private readonly OmdbService _omdb = new();
    private readonly HttpClient _http = new();

    public HomePage()
    {
        InitializeComponent();
        Loaded += async (s, e) => await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var movies = await _omdb.GetPopularAsync();
        await FillRowAsync(MoviesRow, movies);
    }

    private async Task FillRowAsync(StackPanel row, List<Series> items)
    {
        row.Children.Clear();
        foreach (var item in items)
        {
            var card = await BuildPosterCardAsync(item);
            row.Children.Add(card);
        }
    }

    private async Task<Border> BuildPosterCardAsync(Series movie)
    {
        
        Avalonia.Media.IImage? poster = null;
        if (!string.IsNullOrEmpty(movie.PosterUrl) && movie.PosterUrl != "N/A")
        {
            try
            {
                var bytes = await _http.GetByteArrayAsync(movie.PosterUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                poster = new Bitmap(ms);
            }
            catch { }
        }

        
        Control posterControl;
        if (poster != null)
        {
            posterControl = new Image
            {
                Source = poster,
                Width = 110,
                Height = 155,
                Stretch = Stretch.UniformToFill
            };
        }
        else
        {
            posterControl = new Border
            {
                Width = 110,
                Height = 155,
                Background = Brush.Parse("#1a1a2e"),
                Child = new TextBlock
                {
                    Text = movie.Title,
                    Foreground = Brushes.White,
                    FontSize = 11,
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Margin = new Avalonia.Thickness(6),
                    VerticalAlignment = VerticalAlignment.Bottom
                }
            };
        }

        var posterBorder = new Border
        {
            Width = 110,
            Height = 155,
            CornerRadius = new Avalonia.CornerRadius(8),
            ClipToBounds = true,
            Child = posterControl
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
                Text = $"★ {movie.Rating}",
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
            HorizontalAlignment = HorizontalAlignment.Right
        };
        favBtn.Click += (s, e) =>
        {
            movie.IsFavorite = !movie.IsFavorite;
            favBtn.Content = movie.IsFavorite ? "❤️" : "🤍";
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
            Children = { posterBorder, titleBlock, bottomRow }
        };

        
        var cardBorder = new Border
        {
            Child = card,
            Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand)
        };

        cardBorder.PointerPressed += (s, e) =>
        {
            var detailPage = new MovieDetailPage(movie, () =>
            {
                var catalog = TopLevel.GetTopLevel(this) as Window;
                if (catalog is CatalogWindow cw)
                    cw.NavigateToPage(this);
            });

            var catalog = TopLevel.GetTopLevel(this) as Window;
            if (catalog is CatalogWindow cw)
                cw.NavigateToPage(detailPage);
        };

        return cardBorder;
    }
}