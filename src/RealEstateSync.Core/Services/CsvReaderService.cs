using CsvHelper;
using CsvHelper.Configuration;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace RealEstateSync.Core.Services
{
    public class CsvReaderService: ICsvReader
    {
        public async Task<IReadOnlyList<RealEstateItem>> ReadAsync(
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            var items = new List<RealEstateItem>();

            using var reader = new StreamReader(stream, leaveOpen: true);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
                IgnoreBlankLines = true,
                BadDataFound = null
            });

            var records = csv.GetRecordsAsync<CsvRealEstateRow>();

            await foreach (var record in records.WithCancellation(cancellationToken))
            {
                if (string.IsNullOrWhiteSpace(record.Code) ||
                    string.IsNullOrWhiteSpace(record.Address))
                {
                    continue;
                }

                items.Add(new RealEstateItem
                {
                    Code = record.Code.Trim(),
                    Address = record.Address.Trim(),
                    Neighborhood = record.Neighborhood?.Trim() ?? string.Empty,
                    City = record.City?.Trim() ?? string.Empty,
                    State = record.State?.Trim() ?? string.Empty
                });
            }

            return items;
        }

        private sealed class CsvRealEstateRow
        {
            public string Code { get; set; } = string.Empty;
            public string Address { get; set; } = string.Empty;
            public string? Neighborhood { get; set; }
            public string? City { get; set; }
            public string? State { get; set; }
        }

    }
}
