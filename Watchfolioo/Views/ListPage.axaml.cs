namespace Watchfolioo.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Layout;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Watchfolioo.Models;

public partial class ListPage : UserControl
{
    private ObservableCollection<Series> _movies = new();
    private Series? _editingMovie = null;

    public ListPage()
    {
        InitializeComponent();
    }

    public void LoadMovies(List<Series> movies, string title = "Каталог")
    {
        PageTitle.Text = title;
        _movies = new ObservableCollection<Series>(movies);
        RenderMovies();
    }

    private void RenderMovies()
    {
        MovieList.Children.Clear();

        if (_movies.Count == 0)
        {
            MovieList.Children.Add(new TextBlock
            {
                Text = "Список порожній",
                Foreground = Brushes.Gray,
                FontSize = 15,
                Margin = new Thickness(0, 20, 0, 0)
            });
            return;
        }

        foreach (var movie in _movies)
            MovieList.Children.Add(BuildCard(movie));
    }

    private Border BuildCard(Series movie)
    {
        var title = new TextBlock
        {
            Text = movie.Title,
            Foreground = Brushes.White,
            FontSize = 15,
            FontWeight = FontWeight.Medium
        };

        var badge = new Border
        {
            Background = Brush.Parse("#E50914"),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(6, 2),
            Child = new TextBlock { Text = movie.Type, Foreground = Brushes.White, FontSize = 11 }
        };

        var rating = new TextBlock
        {
            Text = $"★ {movie.Rating}",
            Foreground = Brush.Parse("#FFD700"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };

        var genre = new TextBlock
        {
            Text = movie.Genre,
            Foreground = Brush.Parse("#888888"),
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0)
        };

        var infoRow = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 6, 0, 0),
            Children = { badge, rating, genre }
        };

        var textStack = new StackPanel { Children = { title, infoRow } };

        var editBtn = new Button
        {
            Content = "✏️",
            Background = Brushes.Transparent,
            FontSize = 16,
            Padding = new Thickness(4)
        };
        editBtn.Click += (s, e) => StartEdit(movie);

        var deleteBtn = new Button
        {
            Content = "🗑️",
            Background = Brushes.Transparent,
            FontSize = 16,
            Padding = new Thickness(4)
        };
        deleteBtn.Click += (s, e) =>
        {
            _movies.Remove(movie);
            RenderMovies();
        };

        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Children = { editBtn, deleteBtn }
        };

        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
        Grid.SetColumn(textStack, 0);
        Grid.SetColumn(btnPanel, 1);
        grid.Children.Add(textStack);
        grid.Children.Add(btnPanel);

        return new Border
        {
            Background = Brush.Parse("#1E1E1E"),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(14),
            Margin = new Thickness(0, 0, 0, 8),
            Child = grid
        };
    }

    private void AddMovie_OnClick(object? sender, RoutedEventArgs e)
    {
        _editingMovie = null;
        FormTitle.Text = "Новий фільм";
        InputTitle.Text = "";
        InputGenre.Text = "";
        InputRating.Text = "";
        TitleError.IsVisible = false;
        RatingError.IsVisible = false;
        EditForm.IsVisible = true;
    }

    private void StartEdit(Series movie)
    {
        _editingMovie = movie;
        FormTitle.Text = "Редагувати фільм";
        InputTitle.Text = movie.Title;
        InputGenre.Text = movie.Genre;
        InputRating.Text = movie.Rating;
        TitleError.IsVisible = false;
        RatingError.IsVisible = false;
        EditForm.IsVisible = true;
    }

    private void SaveMovie_OnClick(object? sender, RoutedEventArgs e)
    {
        bool valid = true;

        if (string.IsNullOrWhiteSpace(InputTitle.Text))
        {
            TitleError.IsVisible = true;
            InputTitle.BorderThickness = new Thickness(2);
            InputTitle.BorderBrush = Brushes.Red;
            valid = false;
        }
        else
        {
            TitleError.IsVisible = false;
            InputTitle.BorderThickness = new Thickness(0);
        }

        if (!string.IsNullOrWhiteSpace(InputRating.Text))
        {
            if (!double.TryParse(InputRating.Text, out double r) || r < 0 || r > 10)
            {
                RatingError.IsVisible = true;
                InputRating.BorderThickness = new Thickness(2);
                InputRating.BorderBrush = Brushes.Red;
                valid = false;
            }
            else
            {
                RatingError.IsVisible = false;
                InputRating.BorderThickness = new Thickness(0);
            }
        }

        if (!valid) return;

        if (_editingMovie != null)
        {
            _editingMovie.Title = InputTitle.Text ?? "";
            _editingMovie.Genre = InputGenre.Text ?? "";
            _editingMovie.Rating = InputRating.Text ?? "";
        }
        else
        {
            _movies.Add(new Series
            {
                Title = InputTitle.Text ?? "",
                Genre = InputGenre.Text ?? "",
                Rating = InputRating.Text ?? "",
                Type = "Фільм"
            });
        }

        EditForm.IsVisible = false;
        RenderMovies();
    }

    private void CancelEdit_OnClick(object? sender, RoutedEventArgs e)
    {
        EditForm.IsVisible = false;
        TitleError.IsVisible = false;
        RatingError.IsVisible = false;
    }
}