using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
    public interface ISearchProvider
    {
        string Name { get; }

        Task<SearchResult> SearchAsync(
            RealEstateItem item,
            CancellationToken cancellationToken = default);
    }
}
