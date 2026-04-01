using Microsoft.AspNetCore.Mvc;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Core.Models;
using RealEstateSync.Web.Models;

namespace RealEstateSync.Web.Controllers
{
    public class SearchController : Controller
    {
        private readonly ICsvReader _csvReader;
        private readonly ISearchOrchestrator _searchOrchestrator;
        private readonly ISearchHistoryRepository _searchHistoryRepository;

        public SearchController(
            ICsvReader csvReader,
            ISearchOrchestrator searchOrchestrator,
            ISearchHistoryRepository searchHistoryRepository)
        {
            _csvReader = csvReader;
            _searchOrchestrator = searchOrchestrator;
            _searchHistoryRepository = searchHistoryRepository;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload a valid CSV file.");
                return View();
            }

            await using var stream = file.OpenReadStream();
            var items = await _csvReader.ReadAsync(stream, cancellationToken);

            if (!items.Any())
            {
                ModelState.AddModelError(string.Empty, "The file has no valid rows.");
                return View();
            }

            var results = await _searchOrchestrator.SearchAsync(items, cancellationToken);

            // Calcula agregados para esta execução
            var total = items.Count;
            var found = results.Count(r => r.Status == RealEstateStatus.Found);
            var notFound = results.Count(r => r.Status == RealEstateStatus.NotFound);
            var errors = results.Count(r => r.Status == RealEstateStatus.Error);

            // Salva histórico
            var entry = new SearchHistoryEntry
            {
                FileName = file.FileName,
                SearchDate = DateTime.UtcNow,
                TotalItems = total,
                FoundCount = found,
                NotFoundCount = notFound,
                ErrorCount = errors,
                Notes = "Automatic import"
            };

            await _searchHistoryRepository.AddAsync(entry, cancellationToken);

            //return View("Results", results);


            var vm = new SearchResultsViewModel
            {
                Results = results,
                TotalItems = total,
                FoundItems = found,
                NotFoundItems = notFound,
                ErrorItems = errors
            };

            return View("Results", vm);

        }
    }
}