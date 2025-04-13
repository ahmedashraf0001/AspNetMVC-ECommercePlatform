using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Platform.EF.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string? ImageUrl { get; set; } = "/images/default.png";

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(255)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        [Display(Name = "Phone Number")]
        public override string PhoneNumber { get; set; }

        public bool IsDeleted { get; set; } = false;
        public bool IsExternalLogin { get; set; } = false;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}