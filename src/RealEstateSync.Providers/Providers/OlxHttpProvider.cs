using HtmlAgilityPack;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using System.Collections.Generic;

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



        // ↓↓↓ ADD THIS METHOD INSIDE THE CLASS ↓↓↓
        private static List<OlxListingDto> ExtractAllListings(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Pega todos os cards de anúncio
            var cards = doc.DocumentNode
                .SelectNodes("//section[contains(@class,'olx-adcard')]");

            if (cards == null || cards.Count == 0)
                return new List<OlxListingDto>();

            var listings = new List<OlxListingDto>();

            foreach (var card in cards)
            {
                var titleNode = card.SelectSingleNode(
                    ".//h2[contains(@class,'olx-adcard__title')]");

                var linkNode = card.SelectSingleNode(
                    ".//a[contains(@class,'olx-adcard__link') and @href]");

                var priceNode = card.SelectSingleNode(
                    ".//h3[contains(@class,'olx-adcard__price')]");

                var locationNode = card.SelectSingleNode(
                    ".//p[contains(@class,'olx-adcard__location')]");

                var title = titleNode?.InnerText.Trim() ?? "Title not available";
                var price = priceNode?.InnerText.Trim() ?? "Price not available";
                var location = locationNode?.InnerText.Trim() ?? "Location not available";
                var url = linkNode?.GetAttributeValue("href", "#") ?? "#";

                if (!string.IsNullOrWhiteSpace(url) &&
                    url != "#" &&
                    url.StartsWith("/"))
                {
                    url = "https://www.olx.com.br" + url;
                }

                listings.Add(new OlxListingDto
                {
                    Title = HtmlEntity.DeEntitize(title),
                    Price = HtmlEntity.DeEntitize(price),
                    Url = url,
                    Location = HtmlEntity.DeEntitize(location)
                });
            }

            return listings;
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
                    result.ErrorMessage = "HTML is null. Sample file not found or HTTP blocked.";
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
                    result.Details = "No listings found in the sample HTML.";
                    return result;
                }

                // Rotaciona pelos cards disponíveis baseado no índice do item
                var listing = listings[itemIndex % listings.Count];

                result.Status = RealEstateStatus.Found;
                result.ListingTitle = listing.Title;
                result.ListingPrice = listing.Price;
                result.ListingUrl = listing.Url;
                result.ListingLocation = listing.Location;
                result.Details = $"Listing {(itemIndex % listings.Count) + 1} of {listings.Count} extracted from sample HTML.";
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

        
        private sealed class OlxListingDto
        {
            public string Title { get; init; } = string.Empty;
            public string Price { get; init; } = string.Empty;
            public string Url { get; init; } = string.Empty;
            public string Location { get; init; } = string.Empty;
        }
    }
}