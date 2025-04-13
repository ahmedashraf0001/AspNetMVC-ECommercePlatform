using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    public class UserPageViewModel
    {
        public List<UserRoleViewModel> Users { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalUsers { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }

    }
    public class UserRoleViewModel
    {
        public ApplicationUser User;

        public IList<string> Roles;
        public IList<string> UserRoles { get; set; }
    }
}