using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace E_Commerce_Platform.Services
{
    public class UserService
    {
        private readonly UserRepository Repository;

        public UserService(UserRepository userRepository)
        {
            Repository = userRepository;
        }

        public async Task<UserPageViewModel> LoadUserPageAsync(
            int page = 1,
            int pageSize = 12,
            string search = "",
            string sortBy = "")
        {
            IQueryable<ApplicationUser> query = Repository.GetUserQuery();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search)
                                      || u.Email.Contains(search)
                                      || u.PhoneNumber.Contains(search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "Name" => query.OrderBy(u => u.UserName),
                    "Email" => query.OrderBy(u => u.Email),
                    "Id" => query.OrderBy(u => u.Id),
                    "Date" => query.OrderBy(u => u.DateCreated), 
                    "Phone" => query.OrderBy(u => u.PhoneNumber),
                    _ => query
                };
            }

            int totalUsers = await query.CountAsync();

            List<ApplicationUser> users = await ApplyPagination(query, page, pageSize).ToListAsync();
            List<UserRoleViewModel> pagedUsers = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                pagedUsers.Add(new UserRoleViewModel
                {
                    User = user,
                    UserRoles = await Repository.GetRolesAsync(user.Id, true)
                });
            }

            return new UserPageViewModel
            {
                Users = pagedUsers,
                TotalPages = CalculateTotalPages(totalUsers, pageSize),
                CurrentPage = page,
                TotalUsers = totalUsers,
                SearchQuery = search,
                SortBy = sortBy
            };
        }

        public IQueryable<ApplicationUser> ApplyPagination(IQueryable<ApplicationUser> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private int CalculateTotalPages(int totalUsers, int pageSize)
        {
            return (int)Math.Ceiling(totalUsers / (double)pageSize);
        }


        public async Task<List<ApplicationUser>> GetAllUsersAsync(bool withDeletes)
        {
            return await Repository.GetAllUsersAsync(withDeletes);
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationUser user)
        {
            return await Repository.UpdateAsync(user);
        }

        public async Task<List<UserRoleViewModel>> GetAllUserWithRolesAsync(bool withDeletes)
        {
            return await Repository.GetAllUserWithRolesAsync(withDeletes);
        }

        public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName)
        {
            return await Repository.Manager.GetUsersInRoleAsync(roleName);
        }

        public async Task<UserRoleViewModel> GetUserWithRolesAsync(string userId, bool withDeletes)
        {
            return await Repository.GetUserWithRolesAsync(userId, withDeletes: withDeletes);
        }

        public async Task<int> GetTotalUsers()
        {
            var users = await Repository.GetAllUsersAsync(withDeletes: true);
            return users.Count;
        }

        public async Task<IdentityResult> AssignRolesAsync(string userId, List<string> selectedRoles)
        {
            var userWithRoles = await Repository.GetUserWithRolesAsync(userId, true);
            if (userWithRoles == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var user = userWithRoles.User;
            var existingRoles = userWithRoles.UserRoles;

            IdentityResult removeResult = await Repository.Manager.RemoveFromRolesAsync(user, existingRoles);
            if (!removeResult.Succeeded)
            {
                return removeResult;
            }

            if (selectedRoles != null && selectedRoles.Any())
            {
                IdentityResult addResult = await Repository.Manager.AddToRolesAsync(user, selectedRoles);
                return addResult;
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DisableAsync(string userId)
        {
            IdentityResult res = await Repository.DisableAsync(userId);
            return res;
        }

        public async Task<IdentityResult> RemoveAsync(string userId)
        {
            IdentityResult res = await Repository.RemoveAsync(userId);
            return res;
        }

        public async Task<IdentityResult> UpdatePictureAsync(ApplicationUser user, IFormFile ImageFile, string ExistingImageUrl)
        {
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadsfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/main/Users");
                if (!Directory.Exists(uploadsfolder))
                {
                    Directory.CreateDirectory(uploadsfolder);
                }

                var uniquename = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                var filepath = Path.Combine(uploadsfolder, uniquename);

                using (var stream = new FileStream(filepath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                user.ImageUrl = "/images/main/Users/" + uniquename;
            }
            else if (!string.IsNullOrEmpty(ExistingImageUrl))
            {
                user.ImageUrl = ExistingImageUrl;
            }
            else
            {
                user.ImageUrl = "/images/main/Users/default.jpeg";
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ToggleDisableUserAsync(string userId)
        {
            try
            {
                var User = await Repository.GetUserByIDAsync(userId, withDeletes: true);
                if (User == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Product not found." });
                }
                if (User.IsDeleted == false)
                {
                    Repository.Disable(User);
                }
                else
                {
                    Repository.Enable(User);
                }
                await Repository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Deleting Product" });
            }
            return IdentityResult.Success;
        }
    }
}