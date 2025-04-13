using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly CheckoutMediatorService _checkoutMediatorService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(OrderService orderService, ILogger<OrderController> logger, CheckoutMediatorService checkoutMediatorService)
        {
            _orderService = orderService;
            _logger = logger;
            _checkoutMediatorService = checkoutMediatorService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading Order page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Orders";

            return View(await _orderService.LoadOrderPageAsync(page, pageSize, search, sortBy));
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ChangeStatus(int orderId, int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Admin accessed ChangeStatus page for Order ID: {OrderId}", orderId);
            ViewBag.orderId = orderId;
            ViewData["CurrentPage"] = "Orders";

            return View(await _orderService.LoadOrderPageAsync(page, pageSize, search, sortBy));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangeStatus(OrderStatus status, int orderId, int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Attempting to change status for Order ID: {OrderId} to {Status}", orderId, status);
            var result = await _orderService.ChangeStatusAsync(status, orderId);
            if (status == OrderStatus.Cancelled)
            {
                var model = await _orderService.GetOrderAsync(orderId);
                await _checkoutMediatorService.RefundPaymentAsync(model.PaymentIntentId);
            }
            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully changed status for Order ID: {OrderId} to {Status}", orderId, status);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error changing order status: {ErrorDescription}", error.Description);
                ModelState.AddModelError("", error.Description);
            }

            ViewBag.orderId = orderId;
            ViewData["CurrentPage"] = "Orders";

            return View(await _orderService.LoadOrderPageAsync(page, pageSize, search, sortBy));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int orderId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (currentUserId == null)
            {
                _logger.LogWarning("Unauthorized delete attempt. Redirecting to Login.");
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order ID: {OrderId} not found. Redirecting to Index.", orderId);
                return RedirectToAction("Index");
            }

            var orderUserId = order.User.Id;
            if (!User.IsInRole("Admin") && currentUserId != orderUserId)
            {
                _logger.LogWarning("User {UserId} attempted to delete order {OrderId} without proper authorization.", currentUserId, orderId);
                return Forbid();
            }

            _logger.LogInformation("Deleting Order ID: {OrderId} by User ID: {UserId}", orderId, currentUserId);
            var result = await _orderService.DeleteOrderAsync(orderId);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Error deleting order {OrderId}: {ErrorDescription}", orderId, error.Description);
                    ModelState.AddModelError("", error.Description);
                }
            }
            else
            {
                _logger.LogInformation("Successfully deleted Order ID: {OrderId}", orderId);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Details(int orderId, string context = "", int productId = 0, string roleId = "")
        {
            _logger.LogInformation("Fetching details for Order ID: {OrderId}, Context: {Context}, Product ID: {ProductId}, Role ID: {RoleId}",
                orderId, context, productId, roleId);

            ViewBag.context = context;
            ViewBag.productId = productId;
            ViewBag.roleId = roleId;

            var order = await _orderService.GetOrderAsync(orderId);
            if (order == null)
            {
                _logger.LogWarning("Order ID: {OrderId} not found.", orderId);
                return RedirectToAction("Index");
            }
            ViewData["CurrentPage"] = "Orders Details";

            return View(order);
        }
    }
}