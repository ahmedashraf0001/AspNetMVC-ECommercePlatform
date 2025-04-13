using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Fetching all categories.");
            ViewData["CurrentPage"] = "Categories";

            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Navigating to Create Category page.");
            ViewData["CurrentPage"] = "Create Category";

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Category creation failed due to invalid model state.");
                return View(category);
            }

            await _categoryService.AddCategoryAsync(category);
            _logger.LogInformation("Category {CategoryName} created successfully.", category.Name);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Fetching category with ID {CategoryId} for editing.", id);
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found.", id);
                return NotFound();
            }
            ViewData["CurrentPage"] = "Edit Category";

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Category edit failed due to invalid model state.");
                ViewData["CurrentPage"] = "Edit Category";

                return View(category);
            }

            bool updated = await _categoryService.UpdateCategoryAsync(category);
            if (!updated)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found for update.", category.Id);
                return NotFound();
            }

            _logger.LogInformation("Category {CategoryName} updated successfully.", category.Name);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Sort(string sortOption)
        {
            _logger.LogInformation("Sorting categories by {SortOption}.", sortOption);
            var categories = await _categoryService.SortAsync(sortOption);

            ViewBag.SortOption = sortOption;
            ViewData["CurrentPage"] = "Categories";

            return View("Index", categories);
        }

        public async Task<IActionResult> Search(string keyword)
        {
            _logger.LogInformation("Searching categories with keyword: {Keyword}.", keyword);
            var categories = await _categoryService.SearchAsync(keyword);
            ViewData["CurrentPage"] = "Categories";

            return View("Index", categories);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Attempting to delete category with ID {CategoryId}.", id);
            try
            {
                bool deleted = await _categoryService.DeleteCategoryAsync(id);
                if (deleted)
                {
                    _logger.LogInformation("Category with ID {CategoryId} deleted successfully.", id);
                    return Json(new { success = true, message = "Category deleted successfully." });
                }

                _logger.LogWarning("Failed to delete category with ID {CategoryId}.", id);
                return Json(new { success = false, message = "Failed to delete the category." });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning("Cannot delete category with ID {CategoryId}: {ErrorMessage}", id, ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting category with ID {CategoryId}.", id);
                return Json(new { success = false, message = "An unexpected error occurred." });
            }
        }
    }
}