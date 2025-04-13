using E_Commerce_Platform.Services;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly AccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        public ActionResult AccessDenied()
        {
            _logger.LogWarning("Access denied for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewData["CurrentPage"] = "Access Denied";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            _logger.LogInformation("External login initiated via {Provider}", provider);
            var redirecturl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
            var properties = _accountService._signInManager.ConfigureExternalAuthenticationProperties(provider, redirecturl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                _logger.LogError("External login error: {Error}", remoteError);
                ModelState.AddModelError(string.Empty, "Error from external provider.");
                return RedirectToAction("Login");
            }
            var (result, email, fullname, redirectUrl) = await _accountService.HandleExternalLoginAsync(returnUrl, remoteError);

            if (result.Errors.Any(e => e.Description == "User not found, requires registration."))
            {
                _logger.LogInformation("User {Email} not found during external login. Redirecting to ExternalRegister.", email);
                return RedirectToAction("ExternalRegister", new { email, fullname, returnUrl });
            }

            if (!result.Succeeded)
            {
                _logger.LogWarning("External login failed for user: {Email}. Errors: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                ModelState.AddModelError(string.Empty, string.Join(", ", result.Errors.Select(e => e.Description)));
                return RedirectToAction("Login");
            }
            _logger.LogInformation("User {Email} logged in via external provider.", email);
            return RedirectToLocal(null);
        }

        public IActionResult ExternalRegister(string Email, string FullName, string returnUrl = null)
        {
            _logger.LogInformation("User {Email} accessing external registration page.", Email);
            var model = new ExternalRegisterViewModel
            {
                Email = Email,
                FullName = FullName,
                ReturnUrl = returnUrl ?? "/Ecommerce/Home"
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalRegister(ExternalRegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid external registration attempt for {Email}", model.Email);
                return View(model);
            }
            _logger.LogInformation("User attempting external registration: {Email}", model.Email);

            IdentityResult result = await _accountService.ExternalRegisterAsync(model);

            if (!result.Succeeded)
            {
                _logger.LogWarning("External registration failed for user: {Email}. Errors: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
            _logger.LogInformation("User registered successfully via external provider: {Email}", model.Email);

            return RedirectToLocal(model.ReturnUrl);
        }

        [HttpGet]
        public IActionResult Login()
        {
            _logger.LogInformation("Login page accessed.");
            ViewData["CurrentPage"] = "Login";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Invalid login attempt.");
                    return View(model);
                }

                _logger.LogInformation("User attempting to log in: {Email}", model.Email);
                IdentityResult result = await _accountService.LoginAsync(model);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login failed for user: {Email}. Errors: {Errors}",
                        model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                return RedirectToAction("Home", "Ecommerce");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", model.Email);
                return RedirectToAction("Error", "Ecommerce");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User {UserId} logging out.", User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _accountService.Logout();
            return RedirectToAction("home", "Ecommerce");
        }

        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            _logger.LogInformation("User {UserId} logging out.", User.FindFirstValue(ClaimTypes.NameIdentifier));
            await _accountService.Logout();
            return RedirectToAction("home", "Ecommerce");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            _logger.LogInformation("Register page accessed.");
            ViewData["CurrentPage"] = "Register";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid registration attempt.");
                return View(model);
            }
            _logger.LogInformation("User attempting to register: {Email}", model.Email);
            IdentityResult result = await _accountService.RegisterAsync(model);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Registration failed for user: {Email}. Errors: {Errors}", model.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                return HandleRegisterErrors(result, model);
            }

            _logger.LogInformation("User registered successfully: {Email}", model.Email);
            return RedirectToAction("Home", "Ecommerce");
        }
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewData["CurrentPage"] = "Forget Password";
            return View("ForgotPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ForgotPassword model validation failed.");
                return View(model);
            }

            _logger.LogInformation("Processing ForgotPassword request for email: {Email}", model.Email);
            var result = await _accountService.ForgotPasswordAsync(model, Request);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("ForgotPassword failed: {Error}", error.Description);
                }
                return View(model);
            }

            _logger.LogInformation("ForgotPassword email sent successfully to {Email}", model.Email);
            return View("ForgotPasswordConfirmation");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ResetPassword(string userId, string token)
        {
            if (userId == null)
            {
                _logger.LogWarning("ResetPassword attempted with null userId.");
                return RedirectToAction("ForgotPassword");
            }

            _logger.LogInformation("ResetPassword page accessed for userId: {UserId}", userId);
            ViewData["CurrentPage"] = "Reset Password";
            return View(new ResetPasswordViewModel { userId = userId, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ResetPassword model validation failed for userId: {UserId}", model.userId);
                return View("ResetPassword", model);
            }

            _logger.LogInformation("Processing ResetPassword for userId: {UserId}", model.userId);
            var result = await _accountService.ResetPasswordAsync(model);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                    _logger.LogWarning("ResetPassword failed for userId: {UserId}, Error: {Error}", model.userId, error.Description);
                }
                return View("ResetPassword", model);
            }

            _logger.LogInformation("ResetPassword successful for userId: {UserId}. Logging out.", model.userId);
            await _accountService.Logout();
            return View("PasswordResetSuccess");
        }

        private IActionResult HandleRegisterErrors(IdentityResult result, RegisterViewModel model)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View("Register", model);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Home", "Ecommerce");
        }
    }
}