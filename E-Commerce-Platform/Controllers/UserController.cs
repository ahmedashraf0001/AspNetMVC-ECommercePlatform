using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    public enum SortOption
    {
        Name,
        Email,
        Id,
        Date,
        Phone
    }

    [Authorize]
    public class UserController : Controller
    {
        private readonly UserService _user;
        private readonly ILogger<UserController> _logger;

        public UserController(UserService user, ILogger<UserController> logger)
        {
            _user = user;
            _logger = logger;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading User page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Users";

            return View(await _user.LoadUserPageAsync(page, pageSize, search, sortBy));
        }

        public async Task<ActionResult> Details(string userId, string context = "", string roleId = "")
        {
            var currentUserId = GetCurrentUserId();

            if (!User.IsInRole("Admin") && currentUserId != userId)
            {
                _logger.LogWarning("Unauthorized access attempt to user details for user ID: {userId}", userId);
                return Forbid();
            }

            _logger.LogInformation("Fetching details for user ID: {userId}", userId);
            bool withDeletes = User.IsInRole("Admin");
            ViewBag.context = context;
            ViewBag.roleId = roleId;
            ViewData["CurrentPage"] = "Users Details";

            return View(await _user.GetUserWithRolesAsync(userId, withDeletes));
        }

        [HttpGet]
        public async Task<ActionResult> Edit(string userId)
        {
            var currentUserId = GetCurrentUserId();

            if (!User.IsInRole("Admin") && currentUserId != userId)
            {
                _logger.LogWarning("Unauthorized attempt to edit user ID: {userId}", userId);
                return Forbid();
            }
            ViewData["CurrentPage"] = "Edit User";

            _logger.LogInformation("Fetching user data for editing user ID: {userId}", userId);
            bool withDeletes = User.IsInRole("Admin");
            return View(await _user.GetUserWithRolesAsync(userId, withDeletes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user, IFormFile ImageFile, string ExistingImageUrl)
        {
            var currentUserId = GetCurrentUserId();

            if (!User.IsInRole("Admin") && currentUserId != user.Id)
            {
                _logger.LogWarning("Unauthorized attempt to edit user ID: {userId}", user.Id);
                return Forbid();
            }

            _logger.LogInformation("Updating user ID: {userId}", user.Id);
            await _user.UpdatePictureAsync(user, ImageFile, ExistingImageUrl);
            var result = await _user.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("Successfully updated user ID: {userId}", user.Id);
                return RedirectToAction("Details", new { userId = user.Id });
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error updating user ID: {userId}. Error: {error}", user.Id, error.Description);
                ModelState.AddModelError("", error.Description);
            }

            bool withDeletes = User.IsInRole("Admin");
            return View(await _user.GetUserWithRolesAsync(user.Id, withDeletes));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var currentUserId = GetCurrentUserId();

            if (!User.IsInRole("Admin") && currentUserId != userId)
            {
                _logger.LogWarning("Unauthorized attempt to delete user ID: {userId}", userId);
                return Forbid();
            }

            if (User.IsInRole("Admin") && currentUserId == userId)
            {
                _logger.LogWarning("Admin attempted to delete their own account.");
                ModelState.AddModelError("", "Admins cannot delete their own account.");
                return View("Details", await _user.GetUserWithRolesAsync(userId, true));
            }

            _logger.LogInformation("Attempting to delete user ID: {userId}", userId);
            IdentityResult result = await _user.RemoveAsync(userId);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError("Error deleting user ID: {userId}. Errors: {errors}", userId, errors);
                ModelState.AddModelError("", "Error deleting user: " + errors);
                return View("Details", await _user.GetUserWithRolesAsync(userId, User.IsInRole("Admin")));
            }

            _logger.LogInformation("Successfully deleted user ID: {userId}", userId);

            if (userId == currentUserId)
            {
                return RedirectToAction("LogoutGet", "Account");
            }

            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ToggleDisable(string userId)
        {
            _logger.LogInformation("Toggling disable status for user ID: {userId}", userId);
            var result = await _user.ToggleDisableUserAsync(userId);

            if (!result.Succeeded)
            {
                _logger.LogError("Error disabling user ID: {userId}", userId);
                ModelState.AddModelError("", "Error Disabling User");
            }

            return RedirectToAction("Details", new { userId = userId });
        }
    }
}