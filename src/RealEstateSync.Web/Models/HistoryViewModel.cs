using RealEstateSync.Core.Models;

namespace RealEstateSync.Web.Models
{
    public class HistoryViewModel
    {
        public IReadOnlyList<SearchHistoryEntry> Entries { get; set; }
            = new List<SearchHistoryEntry>();

        // Totais agregados de todas as sessões
        public int TotalSessions { get; set; }
        public int TotalItems { get; set; }
        public int TotalFound { get; set; }
        public int TotalNotFound { get; set; }
        public int TotalErrors { get; set; }
    }
}