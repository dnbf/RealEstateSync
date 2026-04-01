using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Models
{
    public enum RealEstateStatus
    {
        Unknown = 0,
        Found = 1,
        NotFound = 2,
        Error = 3
    }

    public class SearchResult
    {
        public string Code { get; set; } = string.Empty;
        public string SearchPortal { get; set; } = string.Empty;
        public RealEstateStatus Status { get; set; }
        public string? Details { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;

        // Novos campos
        public string? ListingTitle { get; set; }
        public string? ListingPrice { get; set; }
        public string? ListingUrl { get; set; }
        public string? ListingLocation { get; set; }

    }
}
