using E_Commerce_Platform.Services;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    public class EcommerceController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<EcommerceController> _logger;

        public EcommerceController(ProductService productService, ILogger<EcommerceController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public IActionResult Home()
        {
            _logger.LogInformation("Navigating to Home page.");
            ViewData["CurrentPage"] = "Home";
            return View();
        }

        public IActionResult Shop()
        {
            _logger.LogInformation("Navigating to Shop page.");
            ViewData["CurrentPage"] = "Shop";
            return RedirectToAction("ShopPage", "Product");
        }

        public IActionResult About()
        {
            _logger.LogInformation("Navigating to About page.");
            ViewData["CurrentPage"] = "About";
            return View();
        }

        public IActionResult Contact()
        {
            _logger.LogInformation("Navigating to Contact page.");
            ViewData["CurrentPage"] = "Contact";
            return View();
        }

        public IActionResult Product()
        {
            _logger.LogInformation("Navigating to Product page.");
            ViewData["CurrentPage"] = "Product";
            return RedirectToAction("ProductPage", "Product");
        }

        [Authorize]
        public IActionResult Cart()
        {
            _logger.LogInformation("Navigating to Cart page.");
            ViewData["CurrentPage"] = "Cart";
            return RedirectToAction("CartPage", "Cart");
        }

        public IActionResult Error(int? statusCode)
        {
            var errorModel = new CustomErrorViewModel();

            if (statusCode.HasValue)
            {
                errorModel.Message = statusCode == 404 ? "Page Not Found" : "An error occurred.";
                errorModel.Details = $"Error Code: {statusCode}";
                _logger.LogWarning("Error {StatusCode}: {Message}", statusCode, errorModel.Message);
            }
            else
            {
                var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                errorModel.Message = "An unexpected error occurred.";
                errorModel.Details = exceptionHandlerPathFeature?.Error.Message;
                _logger.LogError(exceptionHandlerPathFeature?.Error, "Unhandled exception: {Message}", errorModel.Details);
            }

            return View("Error", errorModel);
        }
    }
}