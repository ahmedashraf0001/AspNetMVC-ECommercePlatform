using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(CartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [HttpGet]
        public async Task<IActionResult> CartPage()
        {
            var CurrentId = GetCurrentUserId();
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized access attempt to CartPage.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Loading cart for user {UserId}.", CurrentId);
            var cart = await _cartService.LoadCartAsync(CurrentId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var CurrentId = GetCurrentUserId();
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to add product {ProductId} to cart.", productId);
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("User {UserId} adding product {ProductId} (Quantity: {Quantity}) to cart.", CurrentId, productId, quantity);
            var result = await _cartService.AddToCartAsync(CurrentId, productId, quantity);

            if (result.Succeeded)
            {
                _logger.LogInformation("Product {ProductId} added to cart for user {UserId}.", productId, CurrentId);
                return Json(new { success = true, message = "Product added to cart successfully." });
            }

            _logger.LogError("Failed to add product {ProductId} to cart for user {UserId}. Errors: {Errors}", productId, CurrentId, string.Join(" ", result.Errors.Select(e => e.Description)));
            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int newQuantity)
        {
            var CurrentId = GetCurrentUserId();
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to update product {ProductId} quantity.", productId);
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("User {UserId} updating quantity of product {ProductId} to {NewQuantity}.", CurrentId, productId, newQuantity);
            var result = await _cartService.UpdateQuantityAsync(CurrentId, productId, newQuantity);

            if (result.Succeeded)
            {
                _logger.LogInformation("Quantity of product {ProductId} updated successfully for user {UserId}.", productId, CurrentId);
                return Json(new { success = true, message = "Cart updated successfully." });
            }

            _logger.LogError("Failed to update quantity of product {ProductId} for user {UserId}. Errors: {Errors}", productId, CurrentId, string.Join(" ", result.Errors.Select(e => e.Description)));
            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            var CurrentId = GetCurrentUserId();
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to remove product {ProductId} from cart.", productId);
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("User {UserId} removing product {ProductId} from cart.", CurrentId, productId);
            var result = await _cartService.RemoveFromCartAsync(CurrentId, productId);

            if (result.Succeeded)
            {
                _logger.LogInformation("Product {ProductId} removed from cart for user {UserId}.", productId, CurrentId);
                return Json(new { success = true, message = "Product removed from cart successfully." });
            }

            _logger.LogError("Failed to remove product {ProductId} from cart for user {UserId}. Errors: {Errors}", productId, CurrentId, string.Join(" ", result.Errors.Select(e => e.Description)));
            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var CurrentId = GetCurrentUserId();
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to fetch cart data.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("Fetching cart data for user {UserId}.", CurrentId);
            var cart = await _cartService.LoadCartAsync(CurrentId);
            return Json(new { success = true, data = cart });
        }
    }
}