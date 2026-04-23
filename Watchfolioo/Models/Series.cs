namespace Watchfolioo.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Series : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private string? _title;
    public string? Title
    {
        get => _title;
        set { _title = value; OnPropertyChanged(); }
    }

    private string? _genre;
    public string? Genre
    {
        get => _genre;
        set { _genre = value; OnPropertyChanged(); }
    }

    private string? _type;
    public string? Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    private string? _rating;
    public string? Rating
    {
        get => _rating;
        set { _rating = value; OnPropertyChanged(); }
    }

    private bool _isFavorite;
    public bool IsFavorite
    {
        get => _isFavorite;
        set { _isFavorite = value; OnPropertyChanged(); }
    }

    private string? _description;
    public string? Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    private string? _userComment;
    public string? UserComment
    {
        get => _userComment;
        set { _userComment = value; OnPropertyChanged(); }
    }

    private double _userRating = 0;
    public double UserRating
    {
        get => _userRating;
        set { _userRating = value; OnPropertyChanged(); }
    }

    private int _year;
    public int Year
    {
        get => _year;
        set { _year = value; OnPropertyChanged(); }
    }

    private string? _posterUrl;
    public string? PosterUrl
    {
        get => _posterUrl;
        set { _posterUrl = value; OnPropertyChanged(); }
    }

    private string? _imdbId;
    public string? ImdbId
    {
        get => _imdbId;
        set { _imdbId = value; OnPropertyChanged(); }
    }

    private string? _trailerUrl;
    public string? TrailerUrl
    {
        get => _trailerUrl;
        set { _trailerUrl = value; OnPropertyChanged(); }
    }

    private string? _runtime;
    public string? Runtime
    {
        get => _runtime;
        set { _runtime = value; OnPropertyChanged(); }
    }

    private string? _totalSeasons;
    public string? TotalSeasons
    {
        get => _totalSeasons;
        set { _totalSeasons = value; OnPropertyChanged(); }
    }

    private string? _totalEpisodes;
    public string? TotalEpisodes
    {
        get => _totalEpisodes;
        set { _totalEpisodes = value; OnPropertyChanged(); }
    }

    private string? _episodeRunTime;
    public string? EpisodeRunTime
    {
        get => _episodeRunTime;
        set { _episodeRunTime = value; OnPropertyChanged(); }
    }
}