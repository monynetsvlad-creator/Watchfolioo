using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Watchfolioo.Models;

namespace Watchfolioo.Services;

public class TmdbService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey = "e375e740f9d2edad699b68a499bd37f3";
    private readonly string _baseUrl = "https://api.themoviedb.org/3";
    private readonly string _imageBaseUrl = "https://image.tmdb.org/t/p/w500";

    public async Task<List<Series>> SearchMovies(string query)
    {
        try
        {
            var url = $"{_baseUrl}/search/multi?api_key={_apiKey}&query={Uri.EscapeDataString(query)}&language=uk-UA";
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var results = new List<Series>();

            if (doc.RootElement.TryGetProperty("results", out var resultsArray))
            {
                foreach (var item in resultsArray.EnumerateArray())
                {
                    string mediaType = item.TryGetProperty("media_type", out var mt) ? mt.GetString() : "movie";
                    if (mediaType == "person") continue;

                    string title = item.TryGetProperty("title", out var t) ? t.GetString() : 
                                  (item.TryGetProperty("name", out var n) ? n.GetString() : "Unknown");

                    string date = item.TryGetProperty("release_date", out var rd) ? rd.GetString() : 
                                 (item.TryGetProperty("first_air_date", out var fad) ? fad.GetString() : "");

                    results.Add(new Series
                    {
                        Title = title,
                        Year = !string.IsNullOrEmpty(date) && date.Length >= 4 ? int.Parse(date.Substring(0, 4)) : 0,
                        Rating = item.TryGetProperty("vote_average", out var v) ? v.GetDouble().ToString("0.0") : "0.0",
                        PosterUrl = item.TryGetProperty("poster_path", out var p) && !string.IsNullOrEmpty(p.GetString()) 
                                    ? _imageBaseUrl + p.GetString() : "",
                        ImdbId = item.GetProperty("id").GetInt32().ToString(), 
                        Type = mediaType == "movie" ? "Фільм" : "Серіал",
                        Description = item.TryGetProperty("overview", out var o) ? o.GetString() : ""
                    });
                }
            }
            return results;
        }
        catch { return new List<Series>(); }
    }

    public async Task<Series?> GetMovie(string id)
    {
        var series = await GetDetailsFromEndpoint(id, "tv");
        if (series != null) return series;

        return await GetDetailsFromEndpoint(id, "movie");
    }

    private async Task<Series?> GetDetailsFromEndpoint(string id, string endpoint)
    {
        try
        {
            var url = $"{_baseUrl}/{endpoint}/{id}?api_key={_apiKey}&language=uk-UA";
            var response = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(response);
            var item = doc.RootElement;

            string title = item.TryGetProperty("title", out var t) ? t.GetString() : 
                          (item.TryGetProperty("name", out var n) ? n.GetString() : "Unknown");

            string date = item.TryGetProperty("release_date", out var rd) ? rd.GetString() : 
                         (item.TryGetProperty("first_air_date", out var fad) ? fad.GetString() : "");

            string genres = "";
            if (item.TryGetProperty("genres", out var genresArray))
            {
                var genreList = new List<string>();
                foreach (var g in genresArray.EnumerateArray())
                    genreList.Add(g.GetProperty("name").GetString());
                genres = string.Join(", ", genreList);
            }

            var result = new Series
            {
                Title = title,
                Description = item.TryGetProperty("overview", out var o) ? o.GetString() : "",
                Rating = item.TryGetProperty("vote_average", out var v) ? v.GetDouble().ToString("0.0") : "0.0",
                PosterUrl = item.TryGetProperty("poster_path", out var p) && !string.IsNullOrEmpty(p.GetString()) 
                            ? _imageBaseUrl + p.GetString() : "",
                Year = !string.IsNullOrEmpty(date) && date.Length >= 4 ? int.Parse(date.Substring(0, 4)) : 0,
                Genre = genres,
                Type = endpoint == "movie" ? "Фільм" : "Серіал",
                ImdbId = id
            };

            if (endpoint == "movie")
            {
                if (item.TryGetProperty("runtime", out var rt))
                    result.Runtime = $"{rt.GetInt32()} хв";
            }
            else
            {
                if (item.TryGetProperty("number_of_seasons", out var ns))
                    result.TotalSeasons = $"{ns.GetInt32()} сез.";
                
                if (item.TryGetProperty("number_of_episodes", out var ne))
                    result.TotalEpisodes = $"{ne.GetInt32()} сер.";

                if (item.TryGetProperty("episode_run_time", out var ert) && ert.GetArrayLength() > 0)
                    result.EpisodeRunTime = $"{ert[0].GetInt32()} хв/серія";
            }

            return result;
        }
        catch { return null; }
    }
}