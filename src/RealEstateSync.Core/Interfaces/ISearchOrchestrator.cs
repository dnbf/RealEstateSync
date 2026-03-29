using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
   public interface ISearchOrchestrator
    {
        Task<IReadOnlyList<SearchResult>> SearchAsync(
            IReadOnlyList<RealEstateItem> items,
            CancellationToken cancellationToken = default);
    }
}
