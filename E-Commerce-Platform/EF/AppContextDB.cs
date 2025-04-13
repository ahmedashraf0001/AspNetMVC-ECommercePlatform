using E_Commerce_Platform.EF.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace E_Commerce_Platform.EF
{
    public class AppContextDB : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> orderDetails { get; set; }
        public DbSet<Product> products { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Review> reviews { get; set; }
        public DbSet<Cart> carts { get; set; }
        public DbSet<Like> likes { get; set; }
        public DbSet<Transactions> transactions { get; set; }
        public DbSet<Email> emails { get; set; }
        public DbSet<EmailReply> emailReplies { get; set; }

        public AppContextDB()
        { }

        public AppContextDB(DbContextOptions<AppContextDB> dbContextOptions) : base(dbContextOptions)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=E-Commerce-PlatformDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");
            }
            optionsBuilder.ConfigureWarnings(warnings =>
                  warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Transactions>()
                    .HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Product>()
                    .HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Order>()
                .HasOne(o => o.Transaction)
                .WithOne(t => t.Order)
                .HasForeignKey<Transactions>(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.Review)
                .WithMany(r => r.LikesInfo)
                .HasForeignKey(l => l.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            builder.Entity<ApplicationUser>().HasQueryFilter(p => !p.IsDeleted);

            base.OnModelCreating(builder);
            DatabaseSeeder.Seed(builder);
        }
    }
}