namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Watchfolioo.Models;
using Watchfolioo.Services;
using Watchfolioo.Localization;

public partial class MovieDetailPage : UserControl
{
    private Series _movie;
    private Action? _goBack;
    private int _userRating = 0;
    private readonly TranslateService _translator = new();
    private readonly TmdbService _tmdb = new();

    public MovieDetailPage(Series movie, Action? goBack = null)
    {
        InitializeComponent();
        _movie = movie;
        _goBack = goBack;

        DataContext = _movie;

        Strings.LanguageChanged += ApplyLocalization;
        ApplyLocalization();
        
        Task.Run(() => LoadMovie());
    }

    private void ApplyLocalization()
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
        {
            if (DescriptionHeader != null) DescriptionHeader.Text = Strings.Get("description");
            if (UserRatingHeader != null)  UserRatingHeader.Text  = Strings.Get("your_rating");
            if (CommentHeader != null)     CommentHeader.Text     = Strings.Get("your_comment");
            if (SaveBtn != null)           SaveBtn.Content        = Strings.Get("save_review");
            if (BackBtn != null)           BackBtn.Content        = Strings.Get("back");
            if (CommentBox != null)        CommentBox.Watermark   = Strings.Get("write_review");
            if (SavedLabel != null)        SavedLabel.Text        = Strings.Get("review_saved");

            if (_userRating > 0)
                UserRatingLabel.Text = $"{Strings.Get("your_rating")}: {_userRating}/5";
            else
                UserRatingLabel.Text = Strings.Get("not_rated");

            if (_movie.Type == "Фільм" || _movie.Type == "Movie")
            {
                var parts = new List<string?> { Strings.Get("type_movie"), _movie.Runtime };
                MovieType.Text = string.Join(" • ", parts.Where(p => !string.IsNullOrEmpty(p)));
            }
            else
            {
                var parts = new List<string?> 
                { 
                    Strings.Get("type_series"), 
                    _movie.TotalSeasons, 
                    _movie.TotalEpisodes, 
                    _movie.EpisodeRunTime 
                };
                MovieType.Text = string.Join(" • ", parts.Where(p => !string.IsNullOrEmpty(p)));
            }

            var lang = Strings.CurrentLanguage;
            if (!string.IsNullOrEmpty(_movie.Title))
            {
                _movie.Title = await _translator.TranslateAsync(_movie.Title, lang);
            }
            if (!string.IsNullOrEmpty(_movie.Description) && _movie.Description != Strings.Get("loading"))
            {
                _movie.Description = await _translator.TranslateAsync(_movie.Description, lang);
            }
            if (!string.IsNullOrEmpty(_movie.Genre))
            {
                _movie.Genre = await _translator.TranslateAsync(_movie.Genre, lang);
            }
        });
    }

    private async Task LoadMovie()
    {
        _movie.Description = Strings.Get("loading");

        Series? details = await _tmdb.GetMovie(_movie.ImdbId ?? "");

        if (details != null)
        {
            _movie.Rating = details.Rating;
            _movie.PosterUrl = details.PosterUrl;
            _movie.Year = details.Year;
            _movie.Runtime = details.Runtime;
            _movie.TotalSeasons = details.TotalSeasons;
            _movie.TotalEpisodes = details.TotalEpisodes;
            _movie.EpisodeRunTime = details.EpisodeRunTime;
            
            var lang = Strings.CurrentLanguage;
            
            if (!string.IsNullOrEmpty(details.Title))
                _movie.Title = await _translator.TranslateAsync(details.Title, lang);
                
            if (!string.IsNullOrEmpty(details.Description))
                _movie.Description = await _translator.TranslateAsync(details.Description, lang);
            
            if (!string.IsNullOrEmpty(details.Genre))
                _movie.Genre = await _translator.TranslateAsync(details.Genre, lang);

            ApplyLocalization();
        }

        if (!string.IsNullOrEmpty(_movie.PosterUrl))
        {
            try
            {
                using var http = new HttpClient();
                var data = await http.GetByteArrayAsync(_movie.PosterUrl);
                using var ms = new MemoryStream(data);
                var bitmap = new Bitmap(ms);
                
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (MoviePoster != null) MoviePoster.Source = bitmap;
                });
            }
            catch { }
        }
    }

    private void UpdateStars(int count)
    {
        var stars = new[] { Star1, Star2, Star3, Star4, Star5 };
        for (int i = 0; i < stars.Length; i++)
            stars[i].Foreground = i < count ? Brush.Parse("#FFD700") : Brush.Parse("#444444");
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
        SavedLabel.IsVisible = true;
        Task.Delay(2000).ContinueWith(_ => 
            Avalonia.Threading.Dispatcher.UIThread.Post(() => SavedLabel.IsVisible = false));
    }

    private void Back_OnClick(object? sender, RoutedEventArgs e) => _goBack?.Invoke();
}