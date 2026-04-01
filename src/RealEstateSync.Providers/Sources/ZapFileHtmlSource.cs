using RealEstateSync.Core.Interfaces;

namespace RealEstateSync.Providers
{
    public class ZapFileHtmlSource : IHtmlSource
    {
        private readonly string _sampleFilePath;

        public ZapFileHtmlSource(string sampleFilePath)
        {
            _sampleFilePath = sampleFilePath;
        }

        public async Task<string?> GetHtmlAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_sampleFilePath))
                return null;

            return await File.ReadAllTextAsync(
                _sampleFilePath, cancellationToken);
        }
    }
}