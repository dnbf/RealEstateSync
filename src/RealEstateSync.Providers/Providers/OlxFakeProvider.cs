using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Providers.Providers
{
    /// <summary>
    /// Provider fake da OLX para o MVP inicial.
    /// </summary>
    public class OlxFakeProvider: ISearchProvider
    {
        public string Name => "OLX";

        public Task<SearchResult> SearchAsync(
            RealEstateItem item,
            CancellationToken cancellationToken = default)
        {
            var result = new SearchResult
            {
                Code = item.Code,
                SearchPortal = Name,
                SearchDate = DateTime.UtcNow
            };

            // Regra fake:
            // - Código vazio => erro
            // - Último dígito par => Found
            // - Último dígito ímpar => NotFound

            if (string.IsNullOrWhiteSpace(item.Code))
            {
                result.Status = RealEstateStatus.Error;
                result.ErrorMessage = "Invalid code.";
                return Task.FromResult(result);
            }

            var lastChar = item.Code[^1];

            if (!char.IsDigit(lastChar))
            {
                result.Status = RealEstateStatus.Error;
                result.ErrorMessage = "Code does not end with a digit.";
                return Task.FromResult(result);
            }

            var lastDigit = int.Parse(lastChar.ToString());

            if (lastDigit % 2 == 0)
            {
                result.Status = RealEstateStatus.Found;
                result.Details = "Fake: property found on OLX.";
            }
            else
            {
                result.Status = RealEstateStatus.NotFound;
                result.Details = "Fake: property not found on OLX.";
            }

            return Task.FromResult(result);
        }
    }
}
