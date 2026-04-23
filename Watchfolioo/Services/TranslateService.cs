namespace Watchfolioo.Services;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

public class TranslateService
{
    private readonly HttpClient _http = new();
    private readonly Dictionary<string, string> _cache = new();

    public async Task<string> TranslateAsync(string text, string targetLang)
    {
        if (string.IsNullOrEmpty(text)) return text;

        string cacheKey = $"{targetLang}_{text}";
        if (_cache.ContainsKey(cacheKey)) return _cache[cacheKey];

        try
        {
            await Task.Delay(150);

            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
            
            var response = await _http.GetStringAsync(url);
            using var json = JsonDocument.Parse(response);
            
            var result = "";
            var root = json.RootElement;
            
            if (root.GetArrayLength() > 0 && root[0].ValueKind == JsonValueKind.Array)
            {
                var sentences = root[0];
                foreach (var sentence in sentences.EnumerateArray())
                {
                    var translated = sentence[0].GetString();
                    if (translated != null)
                        result += translated;
                }
            }

            var finalResult = string.IsNullOrEmpty(result) ? text : result;
            
            if (!_cache.ContainsKey(cacheKey))
                _cache[cacheKey] = finalResult;

            return finalResult;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translate error: {ex.Message}");
            return text;
        }
    }
}