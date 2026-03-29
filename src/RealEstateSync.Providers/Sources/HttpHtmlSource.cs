using RealEstateSync.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace RealEstateSync.Providers.Sources
{
    /// <summary>
    /// Returns HTML from a real HTTP request.
    /// May be blocked by anti-scraping protections like Cloudflare.
    /// </summary>
    public class HttpHtmlSource : IHtmlSource
    {
        private readonly HttpClient _httpClient;

        public HttpHtmlSource(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _httpClient.DefaultRequestHeaders.Clear();

            _httpClient.DefaultRequestHeaders.Accept.ParseAdd(
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");

            _httpClient.DefaultRequestHeaders.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9,en-US;q=0.8,en;q=0.7");

            _httpClient.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");

            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/146.0.0.0 Safari/537.36");
        }

        public async Task<string?> GetHtmlAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.TooManyRequests)
                return null;

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
    }
}
