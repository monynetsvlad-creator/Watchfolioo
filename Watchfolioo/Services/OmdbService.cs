namespace Watchfolioo.Services;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Watchfolioo.Models;

public class OmdbService
{
    private const string ApiKey = "d1196472";
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
            Console.WriteLine($"SearchMovies відповідь: {response.Substring(0, Math.Min(200, response.Length))}");
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() == "True")
            {
                var items = (JArray)json["Search"];
                foreach (var item in items)
                {
                    list.Add(new Series
                    {
                        Title     = item["Title"]?.ToString(),
                        ImdbId    = item["imdbID"]?.ToString(),
                        PosterUrl = item["Poster"]?.ToString() != "N/A"
                            ? item["Poster"]?.ToString()
                            : ""
                    });
                }
            }
            else
            {
                Console.WriteLine($"SearchMovies помилка: {json["Error"]}");
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
            Console.WriteLine($"GetMovie відповідь: {response.Substring(0, Math.Min(200, response.Length))}");
            var json = JObject.Parse(response);

            if (json["Response"]?.ToString() == "True")
            {
                return new Series
                {
                    Title       = json["Title"]?.ToString(),
                    Description = json["Plot"]?.ToString(),
                    Genre       = json["Genre"]?.ToString(),
                    Rating      = json["imdbRating"]?.ToString(),
                    PosterUrl   = json["Poster"]?.ToString() != "N/A"
                        ? json["Poster"]?.ToString()
                        : "",
                    ImdbId = imdbId
                };
            }
            else
            {
                Console.WriteLine($"GetMovie помилка: {json["Error"]}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetMovie виключення: {ex.Message}");
        }
        return null;
    }
}