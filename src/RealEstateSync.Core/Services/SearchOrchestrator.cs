using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Services
{
    public class SearchOrchestrator: ISearchOrchestrator
    {
        private readonly IEnumerable<ISearchProvider> _providers;

        public SearchOrchestrator(IEnumerable<ISearchProvider> providers)
        {
            _providers = providers;
        }

        public async Task<IReadOnlyList<SearchResult>> SearchAsync(
            IReadOnlyList<RealEstateItem> items,
            CancellationToken cancellationToken = default)
        {
            var results = new List<SearchResult>();

            foreach (var item in items)
            {
                foreach (var provider in _providers)
                {
                    var result = await provider.SearchAsync(item, cancellationToken);
                    results.Add(result);
                }
            }

            return results;
        }

    }
}
