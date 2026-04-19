namespace Watchfolioo.Views;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using Watchfolioo.Models;

public partial class MovieDetailPage : UserControl
{
    private Series _movie;
    private int _userRating = 0;
    private Action? _onBack;

    public MovieDetailPage(Series movie, Action? onBack = null)
    {
        InitializeComponent();
        _movie = movie;
        _onBack = onBack;
        LoadMovie();
    }

    private void LoadMovie()
    {
        MovieTitle.Text = _movie.Title ?? "Без назви";
        MovieType.Text = _movie.Type ?? "Фільм";
        MovieRating.Text = $"★ {_movie.Rating}";
        MovieGenre.Text = _movie.Genre ?? "";
        MovieYear.Text = _movie.Year > 0 ? _movie.Year.ToString() : "";
        MovieDescription.Text = string.IsNullOrEmpty(_movie.Description)
            ? "Опис відсутній"
            : _movie.Description;

        if (!string.IsNullOrEmpty(_movie.UserComment))
            CommentBox.Text = _movie.UserComment;

        if (_movie.UserRating > 0)
        {
            _userRating = (int)_movie.UserRating;
            UpdateStars(_userRating);
            UserRatingLabel.Text = $"Ваша оцінка: {_userRating}/5";
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

    private void Star1_Click(object? sender, RoutedEventArgs e) => SetRating(1);
    private void Star2_Click(object? sender, RoutedEventArgs e) => SetRating(2);
    private void Star3_Click(object? sender, RoutedEventArgs e) => SetRating(3);
    private void Star4_Click(object? sender, RoutedEventArgs e) => SetRating(4);
    private void Star5_Click(object? sender, RoutedEventArgs e) => SetRating(5);

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
        _onBack?.Invoke();
    }
}