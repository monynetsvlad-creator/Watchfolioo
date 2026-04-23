namespace Watchfolioo.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Watchfolioo.Models;

public class OmdbService
{
    private const string ApiKey = "208d9f03";
    private const string BaseUrl = "https://www.omdbapi.com/";
    private HttpClient http = new HttpClient();

    public async Task<List<Series>> SearchMovies(string query)
    {
        var list = new List<Series>();

        try
        {
            var url = BaseUrl + "?apikey=" + ApiKey + "&s=" + query;
            Console.WriteLine($"SearchMovies запит: {url}");

            var response = await http.GetStringAsync(url);
            Console.WriteLine(response);
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() == "True")
            {
                var items = (JArray)json["Search"];

                foreach (var item in items)
                {
                    list.Add(new Series
                    {
                        Title = item["Title"]?.ToString(),
                        ImdbId = item["imdbID"]?.ToString(),
                        PosterUrl = item["Poster"]?.ToString() != "N/A"
                            ? item["Poster"]?.ToString()
                            : ""
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SearchMovies виключення: {ex.Message}");
        }

        return list;
    }

    public async Task<Series?> GetMovie(string imdbId)
    {
        try
        {
            var url = BaseUrl + "?apikey=" + ApiKey + "&i=" + imdbId + "&plot=full";
            Console.WriteLine($"GetMovie запит: {url}");

            var response = await http.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() == "True")
            {
                return new Series
                {
                    Title = json["Title"]?.ToString(),
                    Description = json["Plot"]?.ToString(),
                    Genre = json["Genre"]?.ToString(),
                    Rating = json["imdbRating"]?.ToString(),
                    PosterUrl = json["Poster"]?.ToString(),
                    ImdbId = imdbId,
                    Runtime = json["Runtime"]?.ToString(),
                    TotalSeasons = json["totalSeasons"]?.ToString()
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMovie виключення: {ex.Message}");
        }

        return null;
    }


    public async Task<Dictionary<string, List<Series>>> GetMoviesByGenres()
    {
        var result = new Dictionary<string, List<Series>>();

        var genres = new Dictionary<string, string[]>
        {
            { "🎬 Action", new[] { "John Wick", "Avengers", "Batman", "Mad Max", "Mission Impossible" } },
            { "🧙 Fantasy", new[] { "Harry Potter", "Lord of the Rings", "Narnia", "Fantastic Beasts", "Percy Jackson" } },
            { "🚀 Sci-Fi", new[] { "Interstellar", "Inception", "Dune", "Matrix", "Avatar" } },
            { "😂 Cartoon", new[] { "Shrek", "Frozen", "Toy Story", "Minions", "Kung Fu Panda" } },
            { "😱 Thriller", new[] { "Joker", "Se7en", "Gone Girl", "Shutter Island", "Zodiac" } }
        };

        foreach (var genre in genres)
        {
            var list = new List<Series>();

            foreach (var query in genre.Value)
            {
                var movies = await SearchMovies(query);
                list.AddRange(movies);

                if (list.Count >= 10)
                    break;
            }

            result[genre.Key] = list.Take(10).ToList();
        }

        return result;
    }
}