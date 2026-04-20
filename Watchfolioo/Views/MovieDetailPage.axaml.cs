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

public partial class MovieDetailPage : UserControl
{
    private Series _movie;
    private Action? _goBack;
    private int _userRating = 0;

    public MovieDetailPage(Series movie, Action? goBack = null)
    {
        InitializeComponent();
        _movie = movie;
        _goBack = goBack;
        LoadMovie();
    }

    private async void LoadMovie()
    {
        MovieTitle.Text = _movie.Title ?? "Без назви";
        MovieType.Text = _movie.Type ?? "Фільм";
        MovieRating.Text = $"★ {_movie.Rating}";
        MovieGenre.Text = _movie.Genre ?? "";
        MovieYear.Text = _movie.Year > 0 ? _movie.Year.ToString() : "";
        MovieDescription.Text = string.IsNullOrEmpty(_movie.Description)
            ? "Завантаження..."
            : _movie.Description;

        if (!string.IsNullOrEmpty(_movie.UserComment))
            CommentBox.Text = _movie.UserComment;

        if (_movie.UserRating > 0)
        {
            _userRating = (int)_movie.UserRating;
            UpdateStars(_userRating);
            UserRatingLabel.Text = $"Ваша оцінка: {_userRating}/5";
        }

        if (string.IsNullOrEmpty(_movie.PosterUrl) ||
            string.IsNullOrEmpty(_movie.Description))
        {
            var omdb = new OmdbService();
            Series? details = null;

            // Якщо є ImdbId — завантажуємо напряму
            if (!string.IsNullOrEmpty(_movie.ImdbId))
            {
                details = await omdb.GetMovie(_movie.ImdbId);
            }
            else
            {
                var results = await omdb.SearchMovies(_movie.Title ?? "");
                if (results.Count > 0 && !string.IsNullOrEmpty(results[0].ImdbId))
                    details = await omdb.GetMovie(results[0].ImdbId!);
            }

            if (details != null)
            {
                _movie.PosterUrl   = details.PosterUrl;
                _movie.Description = details.Description;
                _movie.Rating      = details.Rating;
                _movie.Genre       = details.Genre;
                _movie.Year        = details.Year;

                MovieRating.Text      = $"★ {_movie.Rating}";
                MovieGenre.Text       = _movie.Genre ?? "";
                MovieYear.Text        = _movie.Year > 0 ? _movie.Year.ToString() : "";
                MovieDescription.Text = _movie.Description ?? "Опис відсутній";
            }
            else
            {
                MovieDescription.Text = "Опис відсутній";
            }
        }

        // Завантажуємо постер
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

    private void Star1_Click(object? s, RoutedEventArgs e) => SetRating(1);
    private void Star2_Click(object? s, RoutedEventArgs e) => SetRating(2);
    private void Star3_Click(object? s, RoutedEventArgs e) => SetRating(3);
    private void Star4_Click(object? s, RoutedEventArgs e) => SetRating(4);
    private void Star5_Click(object? s, RoutedEventArgs e) => SetRating(5);

    private void SetRating(int rating)
    {
        _userRating = rating;
        _movie.UserRating = rating;
        UpdateStars(rating);
        UserRatingLabel.Text = $"Ваша оцінка: {rating}/5";
    }

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