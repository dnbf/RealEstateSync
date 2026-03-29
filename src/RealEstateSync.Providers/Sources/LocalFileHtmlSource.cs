using System;
using System.Collections.Generic;
using System.Text;

namespace RealEstateSync.Providers.Sources
{

        /// <summary>
        /// Returns HTML from a local sample file.
        /// Used for demo/portfolio purposes to avoid HTTP scraping restrictions.
        /// </summary>
        public class LocalFileHtmlSource : Core.Interfaces.IHtmlSource
        {
            private readonly string _samplePath;

            public LocalFileHtmlSource()
            {
                _samplePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Samples",
                    "olx_sample.html");
            }

            public async Task<string?> GetHtmlAsync(
                string url,
                CancellationToken cancellationToken = default)
            {
                if (!File.Exists(_samplePath))
                    return null;

                return await File.ReadAllTextAsync(_samplePath, cancellationToken);
            }
        }
    
}
