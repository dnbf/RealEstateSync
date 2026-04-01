using HtmlAgilityPack;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;

namespace RealEstateSync.Providers.Providers
{
    public class ZapMockProvider : ISearchProvider
    {
        private readonly IHtmlSource _htmlSource;

        public string Name => "ZAP";

        // <<< ESTE É O CONSTRUTOR QUE FALTAVA
        public ZapMockProvider(IHtmlSource htmlSource)
        {
            _htmlSource = htmlSource;
        }

        public async Task<SearchResult> SearchAsync(
            RealEstateItem item,
            int itemIndex,
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

                if (html == null)
                {
                    result.Status = RealEstateStatus.Error;
                    result.ErrorMessage = "HTML is null. ZAP sample file not found.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(html))
                {
                    result.Status = RealEstateStatus.Error;
                    result.ErrorMessage = "HTML is empty.";
                    return result;
                }

                var listings = ExtractAllListings(html);

                if (listings.Count == 0)
                {
                    result.Status = RealEstateStatus.NotFound;
                    result.Details = "No listings found in ZAP sample HTML.";
                    return result;
                }

                // Rotaciona pelos cards disponíveis
                var listing = listings[itemIndex % listings.Count];

                result.Status = RealEstateStatus.Found;
                result.ListingTitle = listing.Title;
                result.ListingPrice = listing.Price;
                result.ListingUrl = listing.Url;
                result.ListingLocation = listing.Location;
                result.Details =
                    $"Listing {(itemIndex % listings.Count) + 1} of {listings.Count} extracted from ZAP sample HTML.";
            }
            catch (Exception ex)
            {
                result.Status = RealEstateStatus.Error;
                result.ErrorMessage = $"Unexpected error in ZAP provider: {ex.Message}";
            }

            return result;
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static string BuildSearchUrl(RealEstateItem item)
        {
            var raw = $"{item.Address} {item.City}".Trim();
            if (string.IsNullOrWhiteSpace(raw)) raw = item.Code;
            var query = Uri.EscapeDataString(raw);
            return $"https://www.zapimoveis.com.br/venda/imoveis/sp/?q={query}";
        }

        private static List<ZapListingDto> ExtractAllListings(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var cards = doc.DocumentNode
                .SelectNodes("//div[contains(@class,'zap-listing-card')]");

            if (cards == null || cards.Count == 0)
                return new List<ZapListingDto>();

            var listings = new List<ZapListingDto>();

            foreach (var card in cards)
            {
                var titleNode = card.SelectSingleNode(
                    ".//h2[contains(@class,'zap-listing-card__title')]");

                var linkNode = card.SelectSingleNode(
                    ".//a[contains(@class,'zap-listing-card__link') and @href]");

                var priceNode = card.SelectSingleNode(
                    ".//p[contains(@class,'zap-listing-card__price')]");

                var locationNode = card.SelectSingleNode(
                    ".//p[contains(@class,'zap-listing-card__address')]");

                var title = titleNode?.InnerText.Trim() ?? "Title not available";
                var price = priceNode?.InnerText.Trim() ?? "Price not available";
                var location = locationNode?.InnerText.Trim() ?? "Location not available";
                var url = linkNode?.GetAttributeValue("href", "#") ?? "#";

                if (!string.IsNullOrWhiteSpace(url) &&
                    url != "#" &&
                    url.StartsWith("/"))
                {
                    url = "https://www.zapimoveis.com.br" + url;
                }

                listings.Add(new ZapListingDto
                {
                    Title = HtmlEntity.DeEntitize(title),
                    Price = HtmlEntity.DeEntitize(price),
                    Url = url,
                    Location = HtmlEntity.DeEntitize(location)
                });
            }

            return listings;
        }

        private sealed class ZapListingDto
        {
            public string Title { get; init; } = string.Empty;
            public string Price { get; init; } = string.Empty;
            public string Url { get; init; } = string.Empty;
            public string Location { get; init; } = string.Empty;
        }
    }
}