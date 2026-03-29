using HtmlAgilityPack;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;

namespace RealEstateSync.Providers.Providers
{
    public class OlxHttpProvider : ISearchProvider
    {
        private readonly IHtmlSource _htmlSource;

        public string Name => "OLX";

        public OlxHttpProvider(IHtmlSource htmlSource)
        {
            _htmlSource = htmlSource;
        }

        public async Task<SearchResult> SearchAsync(
            RealEstateItem item,
            CancellationToken cancellationToken = default)
        {
            var result = new SearchResult
            {
                Code = item.Code,
                SearchPortal = Name,
                SearchDate = DateTime.UtcNow
            };

            try
            {
                var url = BuildSearchUrl(item);
                var html = await _htmlSource.GetHtmlAsync(url, cancellationToken);

                if (string.IsNullOrWhiteSpace(html))
                {
                    result.Status = RealEstateStatus.Error;
                    result.ErrorMessage = "Could not retrieve HTML content (blocked or file not found).";
                    return result;
                }

                var hasResults = HasAnyListing(html);

                result.Status = hasResults
                    ? RealEstateStatus.Found
                    : RealEstateStatus.NotFound;

                result.Details = hasResults
                    ? "At least one listing found matching the search."
                    : "No listings found for this property.";
            }
            catch (Exception ex)
            {
                result.Status = RealEstateStatus.Error;
                result.ErrorMessage = $"Unexpected error: {ex.Message}";
            }

            return result;
        }

        private static string BuildSearchUrl(RealEstateItem item)
        {
            var raw = $"{item.Address} {item.City}".Trim();
            if (string.IsNullOrWhiteSpace(raw)) raw = item.Code;

            var query = Uri.EscapeDataString(raw);
            return $"https://www.olx.com.br/imoveis/venda/estado-sp?q={query}";
        }

        private static bool HasAnyListing(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Seletor 1: data-ds-component com AdCard
            var nodes = doc.DocumentNode
                .SelectNodes("//*[contains(@data-ds-component, 'AdCard')]");

            if (nodes != null && nodes.Count > 0)
                return true;

            // Seletor 2: links de anúncios OLX
            nodes = doc.DocumentNode
                .SelectNodes("//a[contains(@href, '/imovel/') and contains(@href, 'olx.com.br')]");

            if (nodes != null && nodes.Count > 0)
                return true;

            // Seletor 3: heurística de conteúdo
            return html.Contains("R$", StringComparison.OrdinalIgnoreCase)
                && html.Contains("m²", StringComparison.OrdinalIgnoreCase);
        }
    }
}