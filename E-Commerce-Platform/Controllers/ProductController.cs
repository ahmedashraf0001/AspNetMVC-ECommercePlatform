using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading shop page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Products";

            return View(await _productService.LoadProductPageAsync(page, pageSize, search, sortBy));
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProductPage(int productId)
        {
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("Fetching product page for Product ID: {ProductId} by User ID: {UserId}", productId, currentId);
            var model = await _productService.LoadProductPageAsync(productId, currentId);

            ViewData["CurrentPage"] = model.Product.Name;
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> BestSellingProducts(int n)
        {
            _logger.LogInformation("Fetching top {Count} best-selling products.", n);
            var bestSellingProducts = await _productService.BestSellingProducts(n);
            return PartialView(bestSellingProducts);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ShopPage(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading shop page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Shop";
            return View(await _productService.LoadProductPageAsync(page, pageSize, search, sortBy));
        }

        public async Task<ActionResult> Details(int productId)
        {
            _logger.LogInformation("Fetching details for Product ID: {ProductId}", productId);
            var model = await _productService.GetProductByIdAsync(productId, withDeletes: true);
            if (model == null)
            {
                _logger.LogWarning("Product ID: {ProductId} not found.", productId);
                return NotFound();
            }
            ViewData["CurrentPage"] = "Product Details";

            return View("Details", model);
        }

        [HttpGet]
        public async Task<ActionResult> Add()
        {
            _logger.LogInformation("Admin accessed Add Product page.");
            ViewData["CurrentPage"] = "Add Product";

            ViewBag.Categories = await _productService.GetCategories();
            return View("Add");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Add(Product product, IFormFile ImageFile)
        {
            _logger.LogInformation("Attempting to add a new product: {ProductName}", product.Name);
            if (ModelState.IsValid)
            {
                var result = await _productService.AddProductAsync(product, ImageFile);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully added Product ID: {ProductId}", product.Id);
                    return RedirectToAction("Index");
                }
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Error adding product: {ErrorDescription}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }
            }
            ViewBag.Categories = await _productService.GetCategories();
            ViewData["CurrentPage"] = "Add Product";

            return View("Add", product);
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int productId)
        {
            _logger.LogInformation("Admin accessed Edit page for Product ID: {ProductId}", productId);
            ViewBag.Categories = await _productService.GetCategories();
            var model = await _productService.GetProductByIdAsync(productId, withDeletes: true);
            ViewData["CurrentPage"] = "Edit Product";

            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Product product, IFormFile ImageFile, string ExistingImageUrl)
        {
            _logger.LogInformation("Attempting to update Product ID: {ProductId}", product.Id);
            var result = await _productService.UpdateProductAsync(product, ImageFile, ExistingImageUrl);

            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully updated Product ID: {ProductId}", product.Id);
                return RedirectToAction("Details", new { productId = product.Id });
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error updating product: {ErrorDescription}", error.Description);
                ModelState.AddModelError("", error.Description);
            }
            ViewBag.Categories = await _productService.GetCategories();
            ViewData["CurrentPage"] = "Add Product";

            return View("Edit", product);
        }

        public async Task<ActionResult> ToggleDisable(int productId)
        {
            _logger.LogInformation("Attempting to toggle disable status for Product ID: {ProductId}", productId);
            var result = await _productService.ToggleDisableProductAsync(productId);
            if (!result.Succeeded)
            {
                _logger.LogError("Error disabling Product ID: {ProductId}", productId);
                ModelState.AddModelError("", "Error Disabling Product");
            }
            else
            {
                _logger.LogInformation("Successfully toggled disable status for Product ID: {ProductId}", productId);
            }
            return RedirectToAction("Details", new { productId = productId });
        }

        public async Task<ActionResult> DeleteProduct(int productId)
        {
            _logger.LogInformation("Attempting to delete Product ID: {ProductId}", productId);
            var result = await _productService.RemoveProductAsync(productId);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    _logger.LogError("Error deleting product: {ErrorDescription}", error.Description);
                    ModelState.AddModelError("", error.Description);
                }
                var model = await _productService.GetProductsAsync(withDeletes: true);
                ViewData["CurrentPage"] = "Products";

                return View("Index", model);
            }
            _logger.LogInformation("Successfully deleted Product ID: {ProductId}", productId);
            return RedirectToAction("Index");
        }
    }
}