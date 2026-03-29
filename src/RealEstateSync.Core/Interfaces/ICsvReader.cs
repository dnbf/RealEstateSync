using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
    public interface ICsvReader
    {
        Task<IReadOnlyList<RealEstateItem>> ReadAsync(
            Stream stream,
            CancellationToken cancellationToken = default);
    }
}
