using E_Commerce_Platform.CustomSignIn;
using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.Services;
using Hangfire;
using MailKit.Net.Imap;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using Stripe;

namespace E_Commerce_Platform
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
                options.JsonSerializerOptions.MaxDepth = 64;
            });
            builder.Services.AddDbContext<AppContextDB>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
            });
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(
                options =>
                {
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;
                }).AddEntityFrameworkStores<AppContextDB>()
                  .AddDefaultTokenProviders();

            builder.Services.AddScoped<DashboardService>();

            builder.Services.AddScoped<UserManager<ApplicationUser>, CustomUserManager>();
            builder.Services.AddScoped<SignInManager<ApplicationUser>, CustomSignInManager>();

            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<RoleService>();
            builder.Services.AddScoped<OrderService>();
            builder.Services.AddScoped<EmailService>();
            builder.Services.AddScoped<CategoryService>();
            builder.Services.AddScoped<CartService>();
            builder.Services.AddScoped<LikeService>();
            builder.Services.AddScoped<TransactionService>();
            builder.Services.AddScoped<OrderdetailService>();
            builder.Services.AddScoped<Services.ReviewService>();
            builder.Services.AddScoped<Services.ProductService>();
            builder.Services.AddScoped<Services.AccountService>();
            builder.Services.AddScoped<Services.CheckoutService>();
            builder.Services.AddScoped<CheckoutMediatorService>();


            builder.Services.AddScoped<OrderRepository>();
            builder.Services.AddScoped<LikesRepository>();
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<CartRepository>();
            builder.Services.AddScoped<EmailRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<CategoryRepository>();
            builder.Services.AddScoped<OrderDetailRepository>();
            builder.Services.AddScoped<ProductRepository>();
            builder.Services.AddScoped<ReviewRepository>();
            builder.Services.AddScoped<TransactionsRepository>();

            builder.Services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            builder.Services.AddSingleton<ImapClient>(_ =>
            {
                var client = new ImapClient();
                return client;
            });


            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                googleOptions.CallbackPath = "/signin-google";
            })
            .AddFacebook(facebookOptions =>
            {
                facebookOptions.ClientId = builder.Configuration["Authentication:Facebook:AppId"];
                facebookOptions.ClientSecret = builder.Configuration["Authentication:Facebook:AppSecret"];
                facebookOptions.CallbackPath = "/signin-facebook";
            });

            builder.Services.AddHangfire(config =>
                config.UseSqlServerStorage("Data Source=.;Initial Catalog=E-Commerce-PlatformDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True"));
            builder.Services.AddHangfireServer();

            var columnOptions = new ColumnOptions();
            columnOptions.Store.Remove(StandardColumn.Properties);
            columnOptions.Store.Add(StandardColumn.LogEvent);
            columnOptions.LogEvent.DataLength = 2048;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.MSSqlServer(
                     builder.Configuration.GetConnectionString("DefaultConnection"),
                    sinkOptions: new MSSqlServerSinkOptions
                    {
                        TableName = "LogTable",
                        AutoCreateSqlTable = true
                    },
                    columnOptions: columnOptions)
                .CreateLogger();

            builder.Host.UseSerilog();

            builder.Services.AddHttpContextAccessor();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Ecommerce/Error");
                app.UseStatusCodePagesWithReExecute("/Ecommerce/Error", "?statusCode={0}");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                recurringJobManager.AddOrUpdate(
                    "weekly-promo-email",
                    () => emailService.SendBulkEmailAsync("Limited Time Discount!", "Get up to 50% off on our best-selling products."),
                    Cron.Daily(8)
                );
            }
            using (var scope = app.Services.CreateScope())
            {
                var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                recurringJobManager.AddOrUpdate(
                    "Fetch-emails",
                    () => emailService.SyncEmailsToDatabase(),
                    Cron.Minutely()
                );
            }
            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var task = DatabaseSeeder.SeedAdminUser(serviceProvider);
                task.GetAwaiter().GetResult();
            }

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Ecommerce}/{action=home}/{id?}");

            app.Run();
        }
    }
}