using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TransactionController : Controller
    {
        private readonly TransactionService _transactionService;
        private readonly ILogger<TransactionController> _logger;

        public TransactionController(TransactionService transactionService, ILogger<TransactionController> logger)
        {
            _transactionService = transactionService;
            _logger = logger;
        }
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading Transaction page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Transactions";

            return View(await _transactionService.LoadTransactionPageAsync(page, pageSize, search, sortBy));
        }
        public async Task<IActionResult> Refund(int transId)
        {
            _logger.LogInformation("Processing refund for transaction ID: {transId}", transId);
            var result = await _transactionService.RefundAsync(transId);

            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully refunded transaction ID: {transId}", transId);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Refund failed for transaction ID: {transId}. Error: {error}", transId, error.Description);
                ModelState.AddModelError("", error.Description);
            }

            return View("Index", await _transactionService.GetAllTransactionsAsync());
        }

        public async Task<ActionResult> Details(int transId)
        {
            _logger.LogInformation("Fetching details for transaction ID: {transId}", transId);
            var transaction = await _transactionService.GetTransactionAsync(transId);
            ViewData["CurrentPage"] = "Transaction Details";

            return View("Details", transaction);
        }
    }
}