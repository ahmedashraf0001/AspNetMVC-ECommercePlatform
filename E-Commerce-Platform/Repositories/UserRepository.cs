using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class UserRepository : Repository<ApplicationUser>
    {
        private AppContextDB _cntx;
        public readonly UserManager<ApplicationUser> Manager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRepository(AppContextDB cntx, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : base(cntx)
        {
            _cntx = cntx;
            Manager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ApplicationUser> GetUserByIDAsync(string userId, bool withDeletes)
        {
            if (withDeletes)
            {
                return await _cntx.Users.Include(e => e.Orders).IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == userId);
            }
            return await _cntx.Users.Include(e => e.Orders).FirstOrDefaultAsync(e => e.Id == userId);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync(bool withDeletes)
        {
            if (withDeletes)
            {
                return await _cntx.Users.Include(e => e.Orders).IgnoreQueryFilters().ToListAsync();
            }
            return await _cntx.Users.Include(e => e.Orders).ToListAsync();
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            var existingUser = await Manager.FindByIdAsync(user.Id);

            existingUser.UserName = user.UserName;
            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.DateCreated = user.DateCreated;
            existingUser.Address = user.Address;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.IsDeleted = user.IsDeleted;
            existingUser.ImageUrl = user.ImageUrl;

            return await Manager.UpdateAsync(existingUser);
        }

        public async Task<IdentityResult> RemoveAsync(string userId)
        {
            var result = await Manager.FindByIdAsync(userId);
            if (result == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Could not find the User!" });
            }
            return await Manager.DeleteAsync(result);
        }

        public async Task<IdentityResult> DisableAsync(string userId)
        {
            var result = await Manager.FindByIdAsync(userId);
            if (result == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Could not find the User!" });
            }
            result.IsDeleted = true;
            return await Manager.UpdateAsync(result);
        }

        public async Task<List<UserRoleViewModel>> GetAllUserWithRolesAsync(bool withDeletes)
        {
            List<UserRoleViewModel> model = new List<UserRoleViewModel>();
            var users = await GetAllUsersAsync(withDeletes);
            foreach (var user in users)
            {
                var userRoles = await Manager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel()
                {
                    User = user,
                    UserRoles = userRoles
                });
            }
            return model;
        }
        public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            return await Manager.CheckPasswordAsync(user, password);
        }
        public async Task<string> GenerateTokenAsync(string userId)
        {
            var user = await _cntx.Users.FirstOrDefaultAsync(e => e.Id == userId);
            return await Manager.GeneratePasswordResetTokenAsync(user);
        }
        public async Task<ApplicationUser> FindByEmailAsync(string Email)
        {
            return await Manager.FindByEmailAsync(Email);
        }
        public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await Manager.ChangePasswordAsync(user, currentPassword, newPassword);
        }
        public async Task<UserRoleViewModel> GetUserWithRolesAsync(string userId, bool withDeletes)
        {
            var userRoles = await GetRolesAsync(userId, withDeletes);
            var userWithOrders = await GetUserByIDAsync(userId, withDeletes);
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            UserRoleViewModel model = new UserRoleViewModel()
            {
                User = userWithOrders,
                UserRoles = userRoles,
                Roles = roles
            };
            return model;
        }

        public async Task<IList<string>> GetRolesAsync(string userId, bool withDeletes)
        {
            ApplicationUser user = await GetUserByIDAsync(userId, withDeletes);
            if (user == null)
            {
                return null;
            }
            var userRoles = await Manager.GetRolesAsync(user);
            return userRoles;
        }

        public void Disable(ApplicationUser entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = true;
                _cntx.Users.Update(entity);
            }
        }

        public void Enable(ApplicationUser entity)
        {
            if (entity != null)
            {
                entity.IsDeleted = false;
                _cntx.Users.Update(entity);
            }
        }

        public IQueryable<ApplicationUser> GetUserQuery()
        {
            return _cntx.Users.AsQueryable();
        }
    }
}