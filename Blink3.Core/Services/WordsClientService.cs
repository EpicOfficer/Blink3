using System.Net;
using System.Net.Http.Json;
using Blink3.Core.Caching;
using Blink3.Core.Configuration;
using Blink3.Core.Interfaces;
using Blink3.Core.Models;
using Microsoft.Extensions.Options;

namespace Blink3.Core.Services;

public class WordsClientService : IWordsClientService
{
    private readonly ICachingService _cachingService;
    private readonly HttpClient _httpClient;

    public WordsClientService(HttpClient httpClient,
        ICachingService cachingService,
        IOptions<BlinkConfiguration> config)
    {
        BlinkConfiguration blinkConfig = config.Value;
        _cachingService = cachingService;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://wordsapiv1.p.rapidapi.com");
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Key", blinkConfig.WordsApiKey);
        _httpClient.DefaultRequestHeaders.Add("X-RapidAPI-Host", "wordsapiv1.p.rapidapi.com");
    }

    public async Task<WordDetails?> GetDefinitionAsync(string word, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"{nameof(WordsClientService)}_{word}";

        if (await _cachingService.GetAsync<WordDetails>(cacheKey, cancellationToken) is
            { } cachedWordDetails) return cachedWordDetails;

        string url = $"/words/{word.ToLower().Trim()}/definitions";
        using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        
        response.EnsureSuccessStatusCode();
        WordDetails? wordDetails = await response.Content.ReadFromJsonAsync<WordDetails>(cancellationToken: cancellationToken);
        
        if (wordDetails is not null)
            await _cachingService.SetAsync(cacheKey, wordDetails, cancellationToken: cancellationToken);
        return wordDetails;
    }
}