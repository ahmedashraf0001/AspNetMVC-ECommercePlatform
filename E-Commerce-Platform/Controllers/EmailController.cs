using E_Commerce_Platform.EF;
using E_Commerce_Platform.Services;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace E_Commerce_Platform.Controllers
{
    [Authorize(Roles ="Admin")]
    public class EmailController : Controller
    {
        private readonly EmailService _emailService;
        private readonly ILogger<ProductController> _logger;


        public EmailController(EmailService emailService, ILogger<ProductController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> index(int page = 1, int pageSize = 12, string search = "", string sortBy = "")
        {
            _logger.LogInformation("Loading Email page with Page: {Page}, PageSize: {PageSize}, Search: {Search}, SortBy: {SortBy}", page, pageSize, search, sortBy);
            ViewData["CurrentPage"] = "Emails";

            return View(await _emailService.LoadEmailPageAsync(page, pageSize, search, sortBy));
        }
        public async Task<IActionResult> Details (string emailId)
        {
            _logger.LogInformation("Loading Email Detail page For Email Id: {emailId}", emailId);
            var model = await _emailService.GetEmailAsync(emailId);
            await _emailService.MarkSeen(model);
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendReply(string emailId, string Message)
        {
            var email = await _emailService.GetEmailAsync(emailId);
            if (email == null) return NotFound();

            var model = new ContactFormViewModel
            {
                Email = email.Sender,
                Subject = "Re: " + email.Subject, 
                Message = Message
            };

            await _emailService.SendEmailAsync(model, true, emailId);
            ViewData["CurrentPage"] = "Email";

            TempData["Success"] = "Reply sent successfully!";
            return RedirectToAction("Details", new { emailId });
        }
    }
}