using System.ComponentModel.DataAnnotations;

namespace E_Commerce_Platform.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }
}
