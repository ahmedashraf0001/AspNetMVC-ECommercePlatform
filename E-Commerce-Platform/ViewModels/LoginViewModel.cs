using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Platform.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [MaxLength(255)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}