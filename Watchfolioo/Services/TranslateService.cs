namespace Watchfolioo.Services;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

public class TranslateService
{
    private readonly HttpClient _http = new();

    public async Task<string> TranslateAsync(string text, string targetLang)
    {
        if (string.IsNullOrEmpty(text)) return text;
        
        if (targetLang == "en") return text;

        try
        {
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={targetLang}&dt=t&q={Uri.EscapeDataString(text)}";
            
            var response = await _http.GetStringAsync(url);
            var json = JsonDocument.Parse(response);
            
            var result = "";
            var sentences = json.RootElement[0];
            foreach (var sentence in sentences.EnumerateArray())
            {
                var translated = sentence[0].GetString();
                if (translated != null)
                    result += translated;
            }
            return string.IsNullOrEmpty(result) ? text : result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Translate error: {ex.Message}");
            return text;
        }
    }
}