namespace Watchfolioo.Views;

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

public partial class HomePage : UserControl
{
    private readonly OmdbService _omdb = new();
    private readonly HttpClient _http = new();
    private CatalogWindow? _catalogWindow;

    public HomePage()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            _catalogWindow = TopLevel.GetTopLevel(this) as CatalogWindow;
            await LoadDataAsync();
        };
    }

    private async Task LoadDataAsync()
    {
        var movieTitles = new[] { "Dune", "Oppenheimer", "Joker", "Interstellar", "Inception", "The Dark Knight", "Avatar", "Gladiator", "Titanic", "The Matrix" };
        var seriesTitles = new[] { "The Witcher", "Black Mirror", "Squid Game", "House of the Dragon", "Breaking Bad", "Stranger Things", "Game of Thrones", "Peaky Blinders", "The Crown", "Narcos" };
        var newMovies2026 = new[] { "Captain America Brave New World", "Thunderbolts", "Mission Impossible Final Reckoning", "Jurassic World Rebirth", "Superman", "The Fantastic Four", "Mortal Kombat 2", "Sinners", "Drop", "Warfare" };
        var trending = new[] { "Adolescence", "The Last of Us", "White Lotus", "Severance", "The Bear", "Andor", "Daredevil Born Again", "Paradise", "Alert", "Your Friendly Neighborhood Spider-Man" };
        var newSeasons2026 = new[] { "Stranger Things", "Squid Game", "The Witcher", "Peaky Blinders", "Yellowstone", "Ozark", "Succession", "Ted Lasso", "Euphoria", "The Boys" };
        var actionTitles = new[] { "John Wick", "Mad Max Fury Road", "Die Hard", "Mission Impossible", "Top Gun Maverick", "The Raid", "Heat", "Predator", "Speed", "Face Off" };
        var comedyTitles = new[] { "The Grand Budapest Hotel", "Superbad", "Step Brothers", "Anchorman", "Bridesmaids", "The Hangover", "Dumb and Dumber", "Clueless", "Mean Girls", "Ferris Bueller" };

        var movies = new List<Series>();
        var series = new List<Series>();
        var newMoviesList = new List<Series>();
        var trendingList = new List<Series>();
        var newSeasonsList = new List<Series>();
        var action = new List<Series>();
        var comedy = new List<Series>();

        foreach (var title in movieTitles)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Фільм"; movies.Add(results[0]); }
        }

        foreach (var title in seriesTitles)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Серіал"; series.Add(results[0]); }
        }

        foreach (var title in newMovies2026)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Фільм"; newMoviesList.Add(results[0]); }
        }

        foreach (var title in trending)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Серіал"; trendingList.Add(results[0]); }
        }

        foreach (var title in newSeasons2026)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Серіал"; newSeasonsList.Add(results[0]); }
        }

        foreach (var title in actionTitles)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Фільм"; action.Add(results[0]); }
        }

        foreach (var title in comedyTitles)
        {
            var results = await _omdb.SearchMovies(title);
            if (results.Count > 0) { results[0].Type = "Фільм"; comedy.Add(results[0]); }
        }

        await FillRowAsync(MoviesRow, movies);
        await FillRowAsync(SeriesRow, series);
        await FillRowAsync(NewMoviesRow, newMoviesList);
        await FillRowAsync(TrendingRow, trendingList);
        await FillRowAsync(NewSeasonsRow, newSeasonsList);
        await FillRowAsync(ActionRow, action);
        await FillRowAsync(ComedyRow, comedy);

        // Завантажуємо деталі для кожного фільму
        var omdb = new OmdbService();
        var allLoaded = new List<Series>();
        allLoaded.AddRange(movies);
        allLoaded.AddRange(newMoviesList);
        allLoaded.AddRange(action);
        allLoaded.AddRange(comedy);

        var allSeriesLoaded = new List<Series>();
        allSeriesLoaded.AddRange(series);
        allSeriesLoaded.AddRange(trendingList);
        allSeriesLoaded.AddRange(newSeasonsList);

        foreach (var movie in allLoaded)
        {
            if (!string.IsNullOrEmpty(movie.ImdbId) && string.IsNullOrEmpty(movie.Genre))
            {
                var details = await omdb.GetMovie(movie.ImdbId);
                if (details != null)
                {
                    movie.Genre = details.Genre;
                    movie.Rating = details.Rating;
                    movie.Year = details.Year;
                }
            }
        }

        foreach (var s in allSeriesLoaded)
        {
            if (!string.IsNullOrEmpty(s.ImdbId) && string.IsNullOrEmpty(s.Genre))
            {
                var details = await omdb.GetMovie(s.ImdbId);
                if (details != null)
                {
                    s.Genre = details.Genre;
                    s.Rating = details.Rating;
                    s.Year = details.Year;
                }
            }
        }

        _catalogWindow?.AddMoviesToCatalog(allLoaded);
        _catalogWindow?.AddMoviesToCatalog(allSeriesLoaded);
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
            if (_catalogWindow == null) return;

            var homePageRef = this;
            var detailPage = new MovieDetailPage(movie, () =>
            {
                _catalogWindow.NavigateToPage(homePageRef);
            });
            _catalogWindow.NavigateToPage(detailPage);
        };

        return cardBorder;
    }
}