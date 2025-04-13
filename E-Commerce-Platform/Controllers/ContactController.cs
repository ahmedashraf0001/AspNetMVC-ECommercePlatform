using E_Commerce_Platform.Services;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Platform.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(EmailService emailService, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmail(ContactFormViewModel model)
        {
            try
            {
                _logger.LogInformation("Attempting to send email from {Email} with subject {Subject}.", model.Email, model.Message);

                await _emailService.SendEmailAsync(model, false);

                _logger.LogInformation("Email sent successfully from {Email}.", model.Email);

                return RedirectToAction("Contact", "Ecommerce");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email from {Email}.", model.Email);
                return RedirectToAction("Error", "Ecommerce", new { message = "An error occurred while sending the email." });
            }
        }
    }
}