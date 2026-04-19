using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Watchfolioo.Models;

namespace Watchfolioo.Services;

public class OmdbService
{
    private const string ApiKey = "d1196472";
    private const string BaseUrl = "https://www.omdbapi.com/";
    private readonly HttpClient _http = new();

    public async Task<List<Series>> SearchMoviesAsync(string query)
    {
        var result = new List<Series>();
        try
        {
            var url = $"{BaseUrl}?s={Uri.EscapeDataString(query)}&apikey={ApiKey}";
            var response = await _http.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() != "True") 
                return result;

            var items = json["Search"] as JArray;
            if (items == null) 
                return result;

            foreach (var item in items)
            {
                var type = item["Type"]?.ToString() switch
                {
                    "movie"  => "Фільм",
                    "series" => "Серіал",
                    _        => "Фільм"
                };

                result.Add(new Series
                {
                    Title     = item["Title"]?.ToString() ?? "",
                    Type      = type,
                    Year      = int.TryParse(item["Year"]?.ToString()?.Replace("–", "").Trim(), out int y) ? y : 0,
                    PosterUrl = item["Poster"]?.ToString() ?? "",
                    ImdbId    = item["imdbID"]?.ToString() ?? ""
                });
            }
        }
        catch { }
        return result;
    }

    public async Task<Series?> GetMovieDetailsAsync(string imdbId)
    {
        try
        {
            var url = $"{BaseUrl}?i={imdbId}&apikey={ApiKey}";
            var response = await _http.GetStringAsync(url);
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() != "True") 
                return null;

            var type = json["Type"]?.ToString() switch
            {
                "movie"  => "Фільм",
                "series" => "Серіал",
                _        => "Фільм"
            };

            return new Series
            {
                Title       = json["Title"]?.ToString() ?? "",
                Type        = type,
                Genre       = json["Genre"]?.ToString() ?? "",
                Rating      = json["imdbRating"]?.ToString() ?? "",
                Description = json["Plot"]?.ToString() ?? "",
                PosterUrl   = json["Poster"]?.ToString() ?? "",
                Year        = int.TryParse(json["Year"]?.ToString(), out int y) ? y : 0,
                ImdbId      = imdbId
            };
        }
        catch { }
        return null;
    }

    public async Task<List<Series>> GetPopularAsync()
    {
        var popularIds = new[] 
        { 
            "tt15239678", 
            "tt15398776", 
            "tt1375666", 
            "tt0816692", 
            "tt4154796", 
            "tt0468569" 
        };

        var result = new List<Series>();
        foreach (var id in popularIds)
        {
            var movie = await GetMovieDetailsAsync(id);
            if (movie != null) 
                result.Add(movie);
        }
        return result;
    }
}