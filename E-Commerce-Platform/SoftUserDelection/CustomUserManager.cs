using E_Commerce_Platform.EF.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

public class CustomUserManager : UserManager<ApplicationUser>
{
    public CustomUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger)
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
    }

    public override async Task<ApplicationUser> FindByIdAsync(string userId)
    {
        return await Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
    }

    public override async Task<ApplicationUser> FindByNameAsync(string userName)
    {
        return await Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public override async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        var user = await Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        if (user.IsDeleted)
            throw new Exception("This account is disabled. Contact support for assistance.");

        return user;
    }
}