using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace FreezerBot;

public class ImgurClient
{
    private HttpClient _httpClient;

    public ImgurClient(string userAgent, string clientID)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
        _httpClient.DefaultRequestHeaders.Add("Authorization", "Client-ID " + clientID);
    }

    public async Task<string?> GallerySearchAsync(string query)
    {
        HttpResponseMessage response = await _httpClient.GetAsync("https://api.imgur.com/3/gallery/search?q=" + query);

        if (!response.IsSuccessStatusCode)
            return "";

        var json = await response.Content.ReadAsStringAsync();
        var root = (JContainer)JToken.Parse(json);

        // Black magic im not willing to try and understand.
        var links = root
            .DescendantsAndSelf()
            .OfType<JProperty>()
            .Where(p => p.Name == "link")
            .Select(p => p.Value.Value<string>())
            .ToArray();

        if (links == null || links.Length < 1)
            return "";

        return links[new Random().Next(links.Length)];
    }
}
