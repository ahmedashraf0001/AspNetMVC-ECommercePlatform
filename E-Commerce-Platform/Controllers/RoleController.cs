using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace E_Commerce_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RoleService _role;
        private readonly ILogger<RoleController> _logger;

        public RoleController(RoleManager<IdentityRole> roleManager, RoleService role, ILogger<RoleController> logger)
        {
            _roleManager = roleManager;
            _role = role;
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Admin accessed Role Index page.");
            ViewData["CurrentPage"] = "Role";

            return View(_role.Manager.Roles.ToList());
        }

        public async Task<ActionResult> Search(string keyword, string type, string roleId = "")
        {
            _logger.LogInformation("Admin is searching for {Type} with keyword: {Keyword}.", type, keyword);

            if (string.IsNullOrWhiteSpace(keyword))
            {
                return type == "Role" ? RedirectToAction("Index") : RedirectToAction("Details", new { roleId });
            }

            var model = await _role.SearchAsync(keyword, type, roleId);

            if (type == "User")
            {
                ViewBag.roleId = roleId;
                ViewData["CurrentPage"] = "Role";

                return View("Details", model.Cast<ApplicationUser>().ToList());
            }
            else if (type == "Role")
            {
                ViewData["CurrentPage"] = "Role";

                return View("Index", model.Cast<IdentityRole>().ToList());
            }
            ViewData["CurrentPage"] = "Role";

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Sort(string sortOption, string type, string roleId)
        {
            _logger.LogInformation("Sorting {Type} by {SortOption}.", type, sortOption);

            if (string.IsNullOrWhiteSpace(sortOption))
            {
                return type == "Role" ? RedirectToAction("Index") : RedirectToAction("Details", new { roleId });
            }

            ViewBag.SortOption = sortOption;
            var model = await _role.SortAsync(sortOption, type, roleId);

            if (type == "User")
            {
                ViewBag.roleId = roleId;
                ViewData["CurrentPage"] = "Role";

                return View("Details", model.Cast<ApplicationUser>().ToList());
            }
            else if (type == "Role")
            {
                ViewData["CurrentPage"] = "Role";

                return View("Index", model.Cast<IdentityRole>().ToList());
            }
            ViewData["CurrentPage"] = "Role";

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation("Admin accessed Role Creation page.");
            ViewData["CurrentPage"] = "Create";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string RoleName)
        {
            _logger.LogInformation("Admin is attempting to create a role: {RoleName}.", RoleName);

            var result = await _role.CreateAsync(RoleName);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role '{RoleName}' created successfully.", RoleName);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error creating role '{RoleName}': {ErrorDescription}", RoleName, error.Description);
                ModelState.AddModelError("", error.Description);
            }
            ViewData["CurrentPage"] = "Create";
            return View("Create");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string roleId)
        {
            _logger.LogWarning("Admin is attempting to delete Role ID: {RoleId}.", roleId);

            IdentityResult result = await _role.DeleteAsync(roleId);
            if (result.Succeeded)
            {
                _logger.LogInformation("Role ID {RoleId} deleted successfully.", roleId);
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error deleting role ID {RoleId}: {ErrorDescription}", roleId, error.Description);
                ModelState.AddModelError("", "Error Deleting Role");
            }
            ViewData["CurrentPage"] = "Role";

            return View("Index", _roleManager.Roles.ToList());
        }

        public async Task<IActionResult> Details(string roleId)
        {
            _logger.LogInformation("Admin is viewing details of Role ID: {RoleId}.", roleId);
            ViewBag.roleId = roleId;
            ViewData["CurrentPage"] = "Role Details";

            return View(await _role.DetailsAsync(roleId));
        }

        [HttpGet]
        public async Task<IActionResult> AssignRole(string userId)
        {
            _logger.LogInformation("Admin accessed Assign Role page for User ID: {UserId}.", userId);
            ViewData["CurrentPage"] = "Assign Roles";

            return View(await _role.GetUserWithRolesAsync(userId, withDeletes: true));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, List<string> selectedRoles)
        {
            _logger.LogInformation("Admin is assigning roles to User ID: {UserId}. Roles: {Roles}", userId, string.Join(", ", selectedRoles));

            var result = await _role.AssignRolesAsync(userId, selectedRoles);
            if (result.Succeeded)
            {
                _logger.LogInformation("Roles assigned successfully to User ID: {UserId}.", userId);
                return RedirectToAction("Edit", "User", new { userId });
            }

            foreach (var error in result.Errors)
            {
                _logger.LogError("Error assigning roles to User ID {UserId}: {ErrorDescription}", userId, error.Description);
                ModelState.AddModelError("", error.Description);
            }
            ViewData["CurrentPage"] = "Assign Roles";

            return View("AssignRole", await _role.GetUserWithRolesAsync(userId, withDeletes: false));
        }
    }
}