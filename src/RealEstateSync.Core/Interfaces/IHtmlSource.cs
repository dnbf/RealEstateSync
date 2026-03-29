using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Interfaces
{
    public interface IHtmlSource
    {
        /// <summary>
        /// Returns HTML content for a given search query.
        /// Can be from HTTP or from a local file depending on the implementation.
        /// </summary>
        Task<string?> GetHtmlAsync(string url, CancellationToken cancellationToken = default);
    }
}
