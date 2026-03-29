using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Models
{
    
        public class SearchConfig
        {
            public int Id { get; set; }
            public string PortalName { get; set; } = string.Empty;
            public string BaseUrl { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public bool IsDefault { get; set; } = false;
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime? UpdatedAt { get; set; }
        }
    
}
