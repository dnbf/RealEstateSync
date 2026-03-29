using System.Collections.Generic;
using RealEstateSync.Core.Models;

namespace RealEstateSync.Web.Models
{
    public class HistoryViewModel
    {
        public IReadOnlyList<SearchHistoryEntry> Entries { get; set; } =
            new List<SearchHistoryEntry>();

        public int TotalItems { get; set; }
        public int FoundItems { get; set; }
        public int NotFoundItems { get; set; }
        public int ErrorItems { get; set; }
    }
}
