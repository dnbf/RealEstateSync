using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
    public interface ISearchConfigRepository
    {
        Task<IReadOnlyList<SearchConfig>> GetAllAsync(
            CancellationToken cancellationToken = default);

        Task<SearchConfig?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task AddAsync(
            SearchConfig config,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(
            SearchConfig config,
            CancellationToken cancellationToken = default);

        Task DeleteAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<SearchConfig?> GetDefaultAsync(
            CancellationToken cancellationToken = default);
    }
}
