using Microsoft.EntityFrameworkCore;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using RealEstateSync.Infra.Data;

namespace RealEstateSync.Infra.Repositories
{
    public class SearchHistoryRepository : ISearchHistoryRepository
    {
        private readonly AppDbContext _dbContext;

        public SearchHistoryRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(SearchHistoryEntry entry, CancellationToken cancellationToken = default)
        {
            await _dbContext.SearchHistoryEntries.AddAsync(entry, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<SearchHistoryEntry>> GetRecentAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.SearchHistoryEntries
                .OrderByDescending(x => x.SearchDate)
                .Take(count)
                .ToListAsync(cancellationToken);
        }

        public async Task<(int total, int found, int notFound, int errors)> GetAggregatedStatsAsync(
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default)
        {
            var query = _dbContext.SearchHistoryEntries
                .Where(x => x.SearchDate >= from && x.SearchDate <= to);

            var total = await query.SumAsync(x => x.TotalItems, cancellationToken);
            var found = await query.SumAsync(x => x.FoundCount, cancellationToken);
            var notFound = await query.SumAsync(x => x.NotFoundCount, cancellationToken);
            var errors = await query.SumAsync(x => x.ErrorCount, cancellationToken);

            return (total, found, notFound, errors);
        }
    }
}
