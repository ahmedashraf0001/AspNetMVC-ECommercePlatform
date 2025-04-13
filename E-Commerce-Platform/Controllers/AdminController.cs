using E_Commerce_Platform.Services;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly DashboardService _dashboardService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(DashboardService dashboardService, ILogger<AdminController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task<IActionResult> Dashboard()
        {
            try
            {
                _logger.LogInformation("Loading dashboard for Admin.");
                var model = await _dashboardService.PrepareDashboard(6);
                ViewData["CurrentPage"] = "Dashboard";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading the dashboard.");


                return View("Error", new CustomErrorViewModel
                {
                    Message = "An error occurred while loading the dashboard.",
                    Details = ex.Message,
                    StatusCode = 500
                });
            }
        }

        public IActionResult ManageOrders()
        {
            _logger.LogInformation("Redirecting Admin to Manage Orders.");
            return RedirectToAction("Index", "Order");
        }

        public IActionResult ManageProducts()
        {
            _logger.LogInformation("Redirecting Admin to Manage Products.");
            return RedirectToAction("Index", "Product");
        }

        public IActionResult ManageRoles()
        {
            _logger.LogInformation("Redirecting Admin to Manage Roles.");
            return RedirectToAction("Index", "Role");
        }

        public IActionResult ManageTransactions()
        {
            _logger.LogInformation("Redirecting Admin to Manage Transactions.");
            return RedirectToAction("Index", "Transaction");
        }

        public IActionResult ManageUsers()
        {
            _logger.LogInformation("Redirecting Admin to Manage Users.");
            return RedirectToAction("Index", "User");
        }
        public IActionResult ManageEmails()
        {
            _logger.LogInformation("Redirecting Admin to Manage Emails.");
            return RedirectToAction("Index", "Email");
        }
    }
}