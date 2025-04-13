using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce_Platform.Services
{
    public class RoleService
    {
        private readonly UserService _userService;
        public readonly RoleManager<IdentityRole> Manager;

        public RoleService(UserService userManager, RoleManager<IdentityRole> roleManager)
        {
            _userService = userManager;
            Manager = roleManager;
        }

        public async Task<IdentityResult> AssignRolesAsync(string userId, List<string> selectedRoles)
        {
            return await _userService.AssignRolesAsync(userId, selectedRoles);
        }

        public async Task<UserRoleViewModel> GetUserWithRolesAsync(string userId, bool withDeletes)
        {
            return await _userService.GetUserWithRolesAsync(userId, withDeletes: withDeletes);
        }

        public async Task<List<object>> SearchAsync(string keyword, string type, string roleId = "")
        {
            string loweredKeyword = keyword.ToLower();

            if (type == "User")
            {
                var role = await Manager.FindByIdAsync(roleId);
                if (role == null) return new List<object>();

                var users = await _userService.GetUsersInRoleAsync(role.Name);
                var model = users
                    .Where(e => e.FullName.ToLower().Contains(loweredKeyword)
                             || e.PhoneNumber.ToLower().Contains(loweredKeyword)
                             || e.Id.ToLower().Contains(loweredKeyword))
                    .Cast<object>()
                    .ToList();

                return model;
            }
            else if (type == "Role")
            {
                var model = Manager.Roles
                    .Where(e => e.Id.ToLower().Contains(loweredKeyword)
                             || e.Name.ToLower().Contains(loweredKeyword))
                    .Cast<object>()
                    .ToList();

                return model;
            }

            return new List<object>();
        }

        public async Task<List<object>> SortAsync(string sortOption, string type, string roleId = "")
        {
            if (type == "User")
            {
                var role = await Manager.FindByIdAsync(roleId);
                if (role == null) return new List<object>();

                var users = await _userService.GetUsersInRoleAsync(role.Name);
                var model = sortOption switch
                {
                    "Phone" => users.OrderBy(u => u.PhoneNumber).Cast<object>().ToList(),
                    "Name" => users.OrderBy(u => u.FullName).Cast<object>().ToList(),
                    "Id" => users.OrderBy(u => u.Id).Cast<object>().ToList(),
                    _ => users.Cast<object>().ToList()
                };

                return model;
            }
            else if (type == "Role")
            {
                var roles = Manager.Roles.AsQueryable();
                var model = sortOption switch
                {
                    "Name" => roles.OrderBy(r => r.Name).Cast<object>().ToList(),
                    "Id" => roles.OrderBy(r => r.Id).Cast<object>().ToList(),
                    _ => roles.Cast<object>().ToList()
                };

                return model;
            }
            return new List<object>();
        }

        public async Task<IdentityResult> CreateAsync(string RoleName)
        {
            if (await Manager.RoleExistsAsync(RoleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = $"{RoleName} already exists." });
            }

            return await Manager.CreateAsync(new IdentityRole(RoleName));
        }

        public async Task<IdentityResult> DeleteAsync(string roleId)
        {
            var role = await Manager.FindByIdAsync(roleId);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Could not find the role!" });
            }
            return await Manager.DeleteAsync(role);
        }

        public async Task<IdentityResult> DeleteByNameAsync(string rolename)
        {
            var role = await Manager.FindByNameAsync(rolename);
            if (role == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Could not find the role!" });
            }
            return await Manager.DeleteAsync(role);
        }

        public async Task<List<ApplicationUser>> DetailsAsync(string roleId)
        {
            var role = await Manager.FindByIdAsync(roleId);
            var users = await _userService.GetUsersInRoleAsync(role.Name);
            return users.ToList();
        }
    }
}