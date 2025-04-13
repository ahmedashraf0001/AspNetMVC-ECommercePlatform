using Azure.Core;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using NuGet.Protocol.Core.Types;
using System;
using System.Security.Claims;

namespace E_Commerce_Platform.Services
{
    public class AccountService
    {
        private const string DefaultUserRole = "User";
        public readonly UserManager<ApplicationUser> _userManager;
        private readonly UserRepository _userRepository;
        public readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleService _roleService;
        private readonly EmailService _emailService;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public AccountService(IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, EmailService emailService, UserRepository userRepository, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleService roleService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleService = roleService;
            _userRepository = userRepository;
            _emailService = emailService;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<IdentityResult> LoginAsync(LoginViewModel model)
        {
            ApplicationUser applicationUser;
            try
            {
                applicationUser = await _userManager.FindByEmailAsync(model.Email);
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
            if (applicationUser == null)
                return IdentityResult.Failed(new IdentityError { Description = "Incorrect Email or Password." });

            await UpdateUserImageClaim(applicationUser);
            var result = await _signInManager.PasswordSignInAsync(applicationUser.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return IdentityResult.Success;
            }

            var errors = new List<IdentityError>();

            if (result.IsLockedOut)
                errors.Add(new IdentityError { Description = "Your account is locked out." });

            if (result.IsNotAllowed)
                errors.Add(new IdentityError { Description = "Login is not allowed. Please confirm your email or contact support." });

            if (result.RequiresTwoFactor)
                errors.Add(new IdentityError { Description = "Two-factor authentication is required." });

            if (!errors.Any())
                errors.Add(new IdentityError { Description = "Incorrect Email or Password." });

            return IdentityResult.Failed(errors.ToArray());
        }

        public async Task<(IdentityResult Result, string Email, string FullName, string ReturnUrl)> HandleExternalLoginAsync(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return (IdentityResult.Failed(new IdentityError { Description = $"Error from external provider: {remoteError}" }), null, null, returnUrl);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "External login info not found." }), null, null, returnUrl);
            }

            var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            if (existingUser != null)
            {
                await UpdateUserImageClaim(existingUser);

                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true);
                if (result.Succeeded)
                {
                    return (IdentityResult.Success, existingUser.Email, existingUser.FullName, "/Ecommerce/home");
                }
            }

            string email = info.Principal.FindFirstValue(ClaimTypes.Email);
            string fullName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var existingUserByEmail = await _userManager.FindByEmailAsync(email);

            if (existingUserByEmail != null)
            {
                await _userManager.AddLoginAsync(existingUserByEmail, info);

                await UpdateUserImageClaim(existingUserByEmail);

                await _signInManager.SignInAsync(existingUserByEmail, isPersistent: true);
                return (IdentityResult.Success, existingUserByEmail.Email, existingUserByEmail.FullName, "/Ecommerce/home");
            }

            return (IdentityResult.Failed(new IdentityError { Description = "User not found, requires registration." }), email, fullName, returnUrl);
        }


        private async Task UpdateUserImageClaim(ApplicationUser user)
        {
            var existingImageClaims = (await _userManager.GetClaimsAsync(user))
                .Where(c => c.Type == "ImageUrl");

            foreach (var claim in existingImageClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }
            if (!string.IsNullOrWhiteSpace(user.ImageUrl))
            {
                var imageClaim = new Claim("ImageUrl", user.ImageUrl);
                await _userManager.AddClaimAsync(user, imageClaim);
            }
        }

        public async Task Logout()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            var applicationUser = CreateUser(model);
            try
            {
                var result = await _userManager.CreateAsync(applicationUser, model.Password);

                if (!result.Succeeded)
                {
                    return result;
                }
                    
                var CreateRole = await _roleService.CreateAsync(DefaultUserRole);

                var roleResult = await _userManager.AddToRoleAsync(applicationUser, DefaultUserRole);

                var claimResult = await _userManager.AddClaimAsync(applicationUser, new Claim("ImageUrl", applicationUser.ImageUrl ?? "/images/default.png"));

                if (!roleResult.Succeeded || !claimResult.Succeeded)
                {
                    await _userManager.DeleteAsync(applicationUser);
                    var allErrors = roleResult.Errors.Concat(claimResult.Errors).ToArray();
                    return IdentityResult.Failed(allErrors);
                }
                await _signInManager.SignInAsync(applicationUser, isPersistent: false);

                return IdentityResult.Success;
            }
            catch
            {
                await _userManager.DeleteAsync(applicationUser);

                return IdentityResult.Failed(new IdentityError { Description = "An unexpected error occurred while setting up the user." });
            }
        }
        public async Task<IdentityResult> ForgotPasswordAsync(ForgotPasswordViewModel model, HttpRequest request)
        {
            try
            {
                var user = await _userRepository.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User Not Found." });
                }
                var token = await _userRepository.GenerateTokenAsync(user.Id);
                if (token == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Failed Generating Password Reset Token." });
                }
                var ResetLink = GenerateResetPasswordLink(user.Id, token, request);
                if (ResetLink == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Failed Generating Password Reset Link." });
                }
                var contactform = new ContactFormViewModel
                {
                    Email = user.Email,
                    Name = "Reset Password",
                };
                await _emailService.SendEmailAsync(contactform, false, resetLink: ResetLink);
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "An error occurred. Please try again later." });
            }
        }
        private IUrlHelper GetUrlHelper()
        {
            var actionContext = _actionContextAccessor.ActionContext;
            if (actionContext == null)
            {
                throw new InvalidOperationException("ActionContext is not available. Ensure this method is called within an HTTP request.");
            }
            return _urlHelperFactory.GetUrlHelper(actionContext);
        }
        public string GenerateResetPasswordLink(string userId, string token, HttpRequest request)
        {
            var urlHelper = GetUrlHelper();
            return urlHelper.Action("ResetPassword", "Account",
                new { userId, token }, request.Scheme);
        }
        public async Task InitAdminAccount()
        {
            var adminRole = "Admin";

            try
            {
                var CreateRole = await _roleService.CreateAsync(adminRole);

                var adminEmail = "admin@example.com";
                var adminPassword = "Admin@123";

                var adminUser = await _userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    var newAdmin = new ApplicationUser
                    {
                        UserName = "Admin",
                        FullName = "Admin",
                        Email = adminEmail,
                        EmailConfirmed = true,
                        PasswordHash = adminPassword,
                        Address = "Default",
                        PhoneNumber = "123456789"
                    };

                    var result = await _userManager.CreateAsync(newAdmin, "Admin@123");
                    if (!result.Succeeded)
                    {
                        await _roleService.DeleteByNameAsync(adminRole);
                        throw new Exception($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }

                    await _userManager.AddToRoleAsync(newAdmin, adminRole);
                    await _userManager.AddClaimAsync(newAdmin, new Claim("Role", adminRole));
                }
            }
            catch (Exception ex)
            {
                await _roleService.DeleteByNameAsync(adminRole);
                Console.WriteLine($"Error initializing admin account: {ex.Message}");
                throw;
            }
        }

        public async Task<IdentityResult> ExternalRegisterAsync(ExternalRegisterViewModel model)
        {
            var applicationUser = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Address = model.Address,
                PhoneNumber = model.PhoneNumber,
                IsExternalLogin = true
            };
            try
            {
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "External login info not found." });
                }

                var createUserResult = await _userManager.CreateAsync(applicationUser);
                if (!createUserResult.Succeeded)
                {
                    return createUserResult;
                }

                await _userManager.AddLoginAsync(applicationUser, info);

                var CreateRole = await _roleService.CreateAsync(DefaultUserRole);

                var roleResult = await _userManager.AddToRoleAsync(applicationUser, DefaultUserRole);

                var claimResult = await _userManager.AddClaimAsync(applicationUser, new Claim("ImageUrl", applicationUser.ImageUrl ?? "/images/default.png"));

                if (!roleResult.Succeeded || !claimResult.Succeeded)
                {
                    await _userManager.DeleteAsync(applicationUser);
                    var allErrors = roleResult.Errors.Concat(claimResult.Errors).ToArray();
                    return IdentityResult.Failed(allErrors);
                }

                await _signInManager.SignInAsync(applicationUser, isPersistent: false);
                return IdentityResult.Success;
            }
            catch
            {
                await _userManager.DeleteAsync(applicationUser);

                return IdentityResult.Failed(new IdentityError { Description = "An unexpected error occurred while setting up the user." });
            }
        }
        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userRepository.GetUserByIDAsync(model.userId, false);
            if (user == null) 
            {
                return IdentityResult.Failed(new IdentityError { Description = "User Not Found" });
            }
            var passwordCheck = await _userRepository.CheckPasswordAsync(user, model.OldPassword);
            if (!passwordCheck)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Old Password Not Correct" });
            }
            return await _userRepository.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        }

        public ApplicationUser CreateUser(RegisterViewModel model)
        {
            return new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                ImageUrl = "/images/default.png"
            };
        }
    }
}