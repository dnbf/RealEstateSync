using Microsoft.AspNetCore.Mvc;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Web.Models;

namespace RealEstateSync.Web.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ISearchHistoryRepository _searchHistoryRepository;

        public HistoryController(ISearchHistoryRepository searchHistoryRepository)
        {
            _searchHistoryRepository = searchHistoryRepository;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            // últimos 20 registros
            var entries = await _searchHistoryRepository.GetRecentAsync(20, cancellationToken);

            // estatísticas dos últimos 30 dias (por exemplo)
            var to = DateTime.UtcNow;
            var from = to.AddDays(-30);

            var (total, found, notFound, errors) =
                await _searchHistoryRepository.GetAggregatedStatsAsync(from, to, cancellationToken);

            var vm = new HistoryViewModel
            {
                Entries = entries,
                TotalItems = total,
                FoundItems = found,
                NotFoundItems = notFound,
                ErrorItems = errors
            };

            return View(vm);
        }
    }
}