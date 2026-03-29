using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Core.Models
{
    public class RealEstateItem
    {
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
