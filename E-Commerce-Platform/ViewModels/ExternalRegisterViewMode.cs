using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Platform.ViewModels
{
    public class ExternalRegisterViewModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        public string ReturnUrl { get; set; }
    }
}