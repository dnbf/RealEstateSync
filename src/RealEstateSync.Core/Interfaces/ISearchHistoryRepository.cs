using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
    public interface ISearchHistoryRepository
    {
        Task AddAsync(
           SearchHistoryEntry entry,
           CancellationToken cancellationToken = default);

        Task<IReadOnlyList<SearchHistoryEntry>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default);

        Task<(int total, int found, int notFound, int errors)> GetAggregatedStatsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);
    }
}
