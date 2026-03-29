using System.Collections.Generic;
using RealEstateSync.Core.Models;

namespace RealEstateSync.Web.Models
{
    public class SearchResultsViewModel
    {
        public IReadOnlyList<SearchResult> Results { get; set; } =
            new List<SearchResult>();

        public int TotalItems { get; set; }
        public int FoundItems { get; set; }
        public int NotFoundItems { get; set; }
        public int ErrorItems { get; set; }
    }
}
