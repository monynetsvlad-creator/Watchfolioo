namespace Watchfolioo.Models;

public class Series
{
    public string? Title { get; set; }
    public string? Genre { get; set; }
    public string? Type { get; set; }
    public string? Rating { get; set; }
    public bool IsFavorite { get; set; }

    public string? Description { get; set; }
    public string? UserComment { get; set; }
    public double UserRating { get; set; } = 0;
    public int Year { get; set; }

    
    public string? PosterUrl { get; set; }
    public string? ImdbId { get; set; }
    public string? TrailerUrl { get; set; }
}