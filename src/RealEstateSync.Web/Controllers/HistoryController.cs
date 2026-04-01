using Microsoft.AspNetCore.Mvc;
using RealEstateSync.Core.Interfaces;
using RealEstateSync.Web.Models;

namespace RealEstateSync.Web.Controllers
{
    public class HistoryController : Controller
    {
        private readonly ISearchHistoryRepository _repository;

        public HistoryController(ISearchHistoryRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var entries = await _repository.GetRecentAsync(100, cancellationToken);

            var vm = new HistoryViewModel
            {
                Entries = entries,
                TotalSessions = entries.Count,
                TotalItems = entries.Sum(e => e.TotalItems),
                TotalFound = entries.Sum(e => e.FoundCount),
                TotalNotFound = entries.Sum(e => e.NotFoundCount),
                TotalErrors = entries.Sum(e => e.ErrorCount)
            };

            return View(vm);
        }
    }
}