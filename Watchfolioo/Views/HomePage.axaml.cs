namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using Watchfolioo.Models;
using Watchfolioo.Services;
using Watchfolioo.Localization;

public partial class HomePage : UserControl
{
    private readonly TmdbService _tmdb = new();
    private readonly HttpClient _http = new();
    private CatalogWindow? _catalogWindow;

    public HomePage()
    {
        InitializeComponent();
        Strings.LanguageChanged += ApplyLocalization;
        ApplyLocalization();

        Loaded += async (s, e) =>
        {
            _catalogWindow = TopLevel.GetTopLevel(this) as CatalogWindow;
            await LoadDataAsync();
        };
    }

    private void ApplyLocalization()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            PopularMoviesTitle.Text = Strings.Get("popular_movies");
            PopularSeriesTitle.Text = Strings.Get("popular_series");
            NewMoviesTitle.Text     = Strings.Get("new_movies_2026");
            SearchingNowTitle.Text  = Strings.Get("searching_now");
            NewSeasonsTitle.Text    = Strings.Get("new_seasons_2026");
            TopActionTitle.Text     = Strings.Get("top_action");
            TopComedyTitle.Text     = Strings.Get("top_comedy");
        });
    }

    private async Task LoadDataAsync()
    {
        var movieTitles = new[] { "Dune", "Oppenheimer", "Joker", "Interstellar", "Inception" };
        var seriesTitles = new[] { "The Witcher", "Black Mirror", "Squid Game", "House of the Dragon" };
        var newMovies2026 = new[] { "Captain America Brave New World", "Superman", "The Fantastic Four" };
        var trending = new[] { "The Last of Us", "White Lotus", "Severance", "The Bear" };
        var newSeasons2026 = new[] { "Stranger Things", "Squid Game", "The Boys" };
        var actionTitles = new[] { "John Wick", "Mad Max Fury Road", "Mission Impossible" };
        var comedyTitles = new[] { "The Grand Budapest Hotel", "Superbad", "The Hangover" };

        var movies = await GetListFromApi(movieTitles, "Фільм");
        var series = await GetListFromApi(seriesTitles, "Серіал");
        var newMoviesList = await GetListFromApi(newMovies2026, "Фільм");
        var trendingList = await GetListFromApi(trending, "Серіал");
        var newSeasonsList = await GetListFromApi(newSeasons2026, "Серіал");
        var action = await GetListFromApi(actionTitles, "Фільм");
        var comedy = await GetListFromApi(comedyTitles, "Фільм");

        await FillRowAsync(MoviesRow, movies);
        await FillRowAsync(SeriesRow, series);
        await FillRowAsync(NewMoviesRow, newMoviesList);
        await FillRowAsync(TrendingRow, trendingList);
        await FillRowAsync(NewSeasonsRow, newSeasonsList);
        await FillRowAsync(ActionRow, action);
        await FillRowAsync(ComedyRow, comedy);

        var allLoaded = new List<Series>();
        allLoaded.AddRange(movies);
        allLoaded.AddRange(newMoviesList);
        allLoaded.AddRange(action);
        allLoaded.AddRange(comedy);

        var allSeriesLoaded = new List<Series>();
        allSeriesLoaded.AddRange(series);
        allSeriesLoaded.AddRange(trendingList);
        allSeriesLoaded.AddRange(newSeasonsList);

        _catalogWindow?.AddMoviesToCatalog(allLoaded);
        _catalogWindow?.AddMoviesToCatalog(allSeriesLoaded);
    }

    private async Task<List<Series>> GetListFromApi(string[] titles, string type)
    {
        var list = new List<Series>();
        foreach (var title in titles)
        {
            var results = await _tmdb.SearchMovies(title);
            if (results.Count > 0) 
            { 
                results[0].Type = type; 
                list.Add(results[0]); 
            }
        }
        return list;
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
        if (!string.IsNullOrEmpty(movie.PosterUrl))
        {
            try
            {
                var bytes = await _http.GetByteArrayAsync(movie.PosterUrl);
                using var ms = new System.IO.MemoryStream(bytes);
                poster = new Bitmap(ms);
            }
            catch { }
        }

        Control posterControl = poster != null 
            ? new Image { Source = poster, Width = 110, Height = 155, Stretch = Stretch.UniformToFill }
            : new Border { Width = 110, Height = 155, Background = Brush.Parse("#1a1a2e"), Child = new TextBlock { Text = movie.Title, Foreground = Brushes.White, FontSize = 11, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(6), VerticalAlignment = VerticalAlignment.Bottom } };

        var posterBorder = new Border { Width = 110, Height = 155, CornerRadius = new CornerRadius(8), ClipToBounds = true, Child = posterControl };
        var titleBlock = new TextBlock { Text = movie.Title, Foreground = Brush.Parse("#CCCCCC"), FontSize = 11, TextWrapping = TextWrapping.Wrap, MaxWidth = 110, Margin = new Thickness(2, 4, 2, 0) };
        var badge = new Border { Background = Brush.Parse("#E50914"), CornerRadius = new CornerRadius(3), Padding = new Thickness(5, 2), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(2, 2, 0, 0), Child = new TextBlock { Text = $"★ {movie.Rating}", Foreground = Brushes.White, FontSize = 9 } };

        var favBtn = new Button { Content = movie.IsFavorite ? "❤️" : "🤍", Background = Brushes.Transparent, FontSize = 14, Padding = new Thickness(2), HorizontalAlignment = HorizontalAlignment.Right };
        favBtn.Click += (s, e) => { movie.IsFavorite = !movie.IsFavorite; favBtn.Content = movie.IsFavorite ? "❤️" : "🤍"; };

        var bottomRow = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        Grid.SetColumn(badge, 0); Grid.SetColumn(favBtn, 1);
        bottomRow.Children.Add(badge); bottomRow.Children.Add(favBtn);

        var card = new StackPanel { Width = 110, Children = { posterBorder, titleBlock, bottomRow } };
        var cardBorder = new Border { Child = card, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand) };

        cardBorder.PointerPressed += (s, e) =>
        {
            if (_catalogWindow == null) return;
            var detailPage = new MovieDetailPage(movie, () => { _catalogWindow.NavigateToPage(this); });
            _catalogWindow.NavigateToPage(detailPage);
        };

        return cardBorder;
    }
}