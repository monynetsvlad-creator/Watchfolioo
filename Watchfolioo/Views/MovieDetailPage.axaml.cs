namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net.Http;
using Watchfolioo.Models;
using Watchfolioo.Services;
using Watchfolioo.Localization;

public partial class MovieDetailPage : UserControl
{
    private Series _movie;
    private Action? _goBack;
    private int _userRating = 0;
    private readonly TranslateService _translator = new();

    public MovieDetailPage(Series movie, Action? goBack = null)
    {
        InitializeComponent();
        _movie = movie;
        _goBack = goBack;
        Strings.LanguageChanged += ApplyLocalization;
        ApplyLocalization();
        LoadMovie();
    }

    private void ApplyLocalization()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            if (DescriptionHeader != null) DescriptionHeader.Text = Strings.Get("description");
            if (UserRatingHeader != null) UserRatingHeader.Text   = Strings.Get("your_rating");
            if (CommentHeader != null)    CommentHeader.Text      = Strings.Get("your_comment");
            if (SaveBtn != null)          SaveBtn.Content         = Strings.Get("save_review");
            if (BackBtn != null)          BackBtn.Content         = Strings.Get("back");
            if (CommentBox != null)       CommentBox.Watermark    = Strings.Get("write_review");
            if (SavedLabel != null)       SavedLabel.Text         = Strings.Get("review_saved");

            if (_userRating > 0)
                UserRatingLabel.Text = $"{Strings.Get("your_rating")}: {_userRating}/5";
            else
                UserRatingLabel.Text = Strings.Get("not_rated");

            if (_movie.Type == "Фільм" || _movie.Type == "Movie" || _movie.Type == "Film")
                MovieType.Text = Strings.Get("type_movie");
            else
                MovieType.Text = Strings.Get("type_series");
        });
    }

    private async void LoadMovie()
    {
        MovieTitle.Text      = _movie.Title ?? "";
        MovieRating.Text     = $"★ {_movie.Rating}";
        MovieGenre.Text      = _movie.Genre ?? "";
        MovieYear.Text       = _movie.Year > 0 ? _movie.Year.ToString() : "";
        MovieDescription.Text = Strings.Get("loading");
        MovieRuntime.Text    = !string.IsNullOrEmpty(_movie.Runtime) ? $"⏱ {_movie.Runtime}" : "";
        MovieSeasons.Text    = !string.IsNullOrEmpty(_movie.TotalSeasons)
            ? $"📺 {_movie.TotalSeasons} {Strings.Get("seasons")}" : "";

        if (!string.IsNullOrEmpty(_movie.UserComment))
            CommentBox.Text = _movie.UserComment;

        if (_movie.UserRating > 0)
        {
            _userRating = (int)_movie.UserRating;
            UpdateStars(_userRating);
            UserRatingLabel.Text = $"{Strings.Get("your_rating")}: {_userRating}/5";
        }

        var omdb = new OmdbService();
        Series? details = null;

        if (!string.IsNullOrEmpty(_movie.ImdbId))
            details = await omdb.GetMovie(_movie.ImdbId);
        else if (!string.IsNullOrEmpty(_movie.Title))
        {
            var results = await omdb.SearchMovies(_movie.Title);
            if (results.Count > 0 && !string.IsNullOrEmpty(results[0].ImdbId))
                details = await omdb.GetMovie(results[0].ImdbId!);
        }

        if (details != null)
        {
            if (!string.IsNullOrEmpty(details.Description) && details.Description != "N/A")
                _movie.Description = details.Description;

            if (!string.IsNullOrEmpty(details.Genre) && details.Genre != "N/A")
                _movie.Genre = details.Genre;

            if (!string.IsNullOrEmpty(details.Runtime) && details.Runtime != "N/A")
                _movie.Runtime = details.Runtime;

            if (!string.IsNullOrEmpty(details.TotalSeasons) && details.TotalSeasons != "N/A")
                _movie.TotalSeasons = details.TotalSeasons;

            // 🔥 ВИПРАВЛЕНО ТУТ (було string > int)
            if (!string.IsNullOrEmpty(details.Rating) && details.Rating != "N/A")
                _movie.Rating = details.Rating;

            if (!string.IsNullOrEmpty(details.PosterUrl) && details.PosterUrl != "N/A")
                _movie.PosterUrl = details.PosterUrl;

            if (details.Year > 0)
                _movie.Year = details.Year;

            MovieRating.Text  = $"★ {_movie.Rating}";
            MovieYear.Text    = _movie.Year > 0 ? _movie.Year.ToString() : "";
            MovieRuntime.Text = !string.IsNullOrEmpty(_movie.Runtime) ? $"⏱ {_movie.Runtime}" : "";
            MovieSeasons.Text = !string.IsNullOrEmpty(_movie.TotalSeasons)
                ? $"📺 {_movie.TotalSeasons} {Strings.Get("seasons")}" : "";
        }

        var lang = Strings.CurrentLanguage;

        var descToTranslate = (!string.IsNullOrEmpty(_movie.Description) && _movie.Description != "N/A")
            ? _movie.Description
            : "";

        var genreToTranslate = (!string.IsNullOrEmpty(_movie.Genre) && _movie.Genre != "N/A")
            ? _movie.Genre
            : "";

        var translatedDesc  = await _translator.TranslateAsync(descToTranslate, lang);
        var translatedGenre = await _translator.TranslateAsync(genreToTranslate, lang);

        MovieDescription.Text = string.IsNullOrEmpty(translatedDesc)
            ? Strings.Get("no_description") : translatedDesc;

        MovieGenre.Text = translatedGenre;

        if (!string.IsNullOrEmpty(_movie.PosterUrl) && _movie.PosterUrl != "N/A")
        {
            try
            {
                using var http = new HttpClient();
                var data = await http.GetByteArrayAsync(_movie.PosterUrl);
                using var ms = new MemoryStream(data);
                MoviePoster.Source = new Bitmap(ms);
            }
            catch { }
        }
    }

    private void UpdateStars(int count)
    {
        var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
        for (int i = 0; i < stars.Length; i++)
            stars[i].Foreground = i < count
                ? Brush.Parse("#FFD700")
                : Brush.Parse("#444444");
    }

    private void SetRating(int rating)
    {
        _userRating = rating;
        _movie.UserRating = rating;
        UpdateStars(rating);
        UserRatingLabel.Text = $"{Strings.Get("your_rating")}: {rating}/5";
    }

    private void Star1_Click(object? s, RoutedEventArgs e) => SetRating(1);
    private void Star2_Click(object? s, RoutedEventArgs e) => SetRating(2);
    private void Star3_Click(object? s, RoutedEventArgs e) => SetRating(3);
    private void Star4_Click(object? s, RoutedEventArgs e) => SetRating(4);
    private void Star5_Click(object? s, RoutedEventArgs e) => SetRating(5);

    private void SaveComment_OnClick(object? sender, RoutedEventArgs e)
    {
        _movie.UserComment = CommentBox.Text ?? "";
        SavedLabel.IsVisible = true;

        var timer = new System.Timers.Timer(2000);
        timer.Elapsed += (s, ev) =>
        {
            timer.Stop();
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                SavedLabel.IsVisible = false);
        };
        timer.Start();
    }

    private void Back_OnClick(object? sender, RoutedEventArgs e)
    {
        _goBack?.Invoke();
    }
}