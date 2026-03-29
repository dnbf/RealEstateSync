using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Models
{
    public class SearchHistoryEntry
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime SearchDate { get; set; } = DateTime.UtcNow;
        public int TotalItems { get; set; }
        public int FoundCount { get; set; }
        public int NotFoundCount { get; set; }
        public int ErrorCount { get; set; }
        public string? Notes { get; set; }

    }
}
