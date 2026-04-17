using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Collections.Generic;
using Watchfolioo.Models;

namespace Watchfolioo.Views;

public partial class ListPage : UserControl
{
    public ListPage()
    {
        InitializeComponent();

        MoviesPanel = this.FindControl<WrapPanel>("MoviesPanel");
    }

    public void LoadMovies(List<Series> movies, string title = "Каталог")
    {
        
        if (MoviesPanel == null)
            return;

        MoviesPanel.Children.Clear();

        foreach (var movie in movies)
        {
            var card = new Border
            {
                Width = 150,
                Height = 200,
                Margin = new Thickness(10),
                Background = Brush.Parse("#1a1a2e"),
                CornerRadius = new CornerRadius(10)
            };

            var stack = new StackPanel
            {
                Margin = new Thickness(10)
            };

            stack.Children.Add(new TextBlock
            {
                Text = movie.Title ?? "Без назви",
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                TextWrapping = TextWrapping.Wrap
            });

            stack.Children.Add(new TextBlock
            {
                Text = movie.Genre ?? "",
                Foreground = Brushes.Gray,
                FontSize = 12
            });

            stack.Children.Add(new TextBlock
            {
                Text = "⭐ " + (movie.Rating ?? "0"),
                Foreground = Brushes.Gold
            });

            card.Child = stack;
            MoviesPanel.Children.Add(card);
        }
    }
}