using E_Commerce_Platform.EF.Models;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce_Platform.ViewModels
{
    public class RoleViewModel
    {
        public IdentityRole Role { get; set; }

        public List<ApplicationUser> roleUsers { get; set; }
    }
}