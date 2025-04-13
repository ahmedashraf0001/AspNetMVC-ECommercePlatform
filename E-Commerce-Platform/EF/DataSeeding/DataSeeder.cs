using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.EF
{
    public static class DatabaseSeeder
    {
        private static readonly DateTime SeedBaseDate = new DateTime(2024, 1, 1);

        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Watches" },
                new Category { Id = 2, Name = "Laptops" },
                new Category { Id = 3, Name = "Headphones" },
                new Category { Id = 4, Name = "VR" },
                new Category { Id = 5, Name = "Gaming" }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Wireless Earphones", Price = 99.99m, CategoryId = 3, ImageUrl = "/images/h.png", Description = "High-quality wireless earphones featuring active noise cancellation, deep bass, and crystal-clear audio. Perfect for music lovers and professionals who need a distraction-free experience." },
                new Product { Id = 2, Name = "Gaming Console", Price = 499.99m, CategoryId = 5, ImageUrl = "/images/gam.png", Description = "Next-generation gaming console with ultra-fast load times, stunning 4K graphics, and a vast library of exclusive titles. Includes a wireless controller and support for multiplayer gaming." },
                new Product { Id = 3, Name = "Over-Ear Headphones", Price = 149.99m, CategoryId = 3, ImageUrl = "/images/p9.jpg", Description = "Premium over-ear headphones designed for audiophiles. Features a comfortable, ergonomic design, studio-quality sound, and a long-lasting battery for all-day listening." },
                new Product { Id = 4, Name = "VR Headset", Price = 299.99m, CategoryId = 4, ImageUrl = "/images/p8.png", Description = "Experience virtual reality like never before with this high-resolution VR headset. Features built-in sensors for motion tracking, adjustable lenses, and compatibility with the latest VR games and applications." },
                new Product { Id = 5, Name = "Wireless Headphones", Price = 129.99m, CategoryId = 3, ImageUrl = "/images/p-7.jpg", Description = "Enjoy wireless freedom with these high-fidelity Bluetooth headphones. Designed for comfort and durability, they provide rich sound, deep bass, and a long battery life of up to 30 hours." },
                new Product { Id = 6, Name = "High-Performance Laptop", Price = 1499.99m, CategoryId = 2, ImageUrl = "/images/p-5.jpg", Description = "A powerful laptop equipped with the latest Intel processor, dedicated graphics card, and a high-refresh-rate display, making it ideal for gaming, content creation, and professional work." },
                new Product { Id = 7, Name = "Noise-Canceling Headphones", Price = 179.99m, CategoryId = 3, ImageUrl = "/images/product-img.jpg", Description = "Advanced noise-canceling headphones designed to block out background noise and provide an immersive listening experience. Features high-fidelity sound and an ultra-comfortable design for long listening sessions." },
                new Product { Id = 8, Name = "VR Gaming Kit", Price = 399.99m, CategoryId = 4, ImageUrl = "/images/p-4.jpg", Description = "A complete VR gaming kit that includes a high-resolution headset, motion-tracking controllers, and a collection of immersive VR games. Perfect for gamers looking to step into the world of virtual reality." },
                new Product { Id = 9, Name = "Smartwatch", Price = 249.99m, CategoryId = 1, ImageUrl = "/images/p-3.jpg", Description = "A stylish smartwatch with a sleek design, advanced fitness tracking, heart rate monitoring, and seamless smartphone integration. Stay connected while tracking your health and daily activities." },
                new Product { Id = 10, Name = "Gaming Laptop", Price = 1799.99m, CategoryId = 2, ImageUrl = "/images/p-2.jpg", Description = "A high-end gaming laptop featuring an NVIDIA RTX graphics card, a high-refresh-rate display, and a powerful cooling system. Built for gamers who demand top-tier performance." },
                new Product { Id = 11, Name = "Budget Wireless Headphones", Price = 79.99m, CategoryId = 3, ImageUrl = "/images/p-1.png", Description = "Affordable wireless headphones that offer clear sound quality, deep bass, and a comfortable fit. Perfect for casual listening and long commutes, with a battery life of up to 20 hours." },
                new Product { Id = 12, Name = "Ultra-Thin Laptop", Price = 1199.99m, CategoryId = 2, ImageUrl = "/images/p10.png", Description = "A sleek and lightweight ultra-thin laptop designed for professionals on the go. Features a high-resolution display, long battery life, and a premium aluminum chassis for durability." },
                new Product { Id = 13, Name = "Smartwatch Pro", Price = 299.99m, CategoryId = 1, ImageUrl = "/images/p12.png", Description = "An advanced smartwatch packed with fitness tracking features, built-in GPS, sleep monitoring, and water resistance. Ideal for athletes and health-conscious users who need a reliable wearable." },
                new Product { Id = 14, Name = "Premium Smartwatch", Price = 349.99m, CategoryId = 1, ImageUrl = "/images/w.png", Description = "A high-end smartwatch with an AMOLED display, premium build quality, and advanced health tracking features. Offers seamless integration with both Android and iOS devices." },
                new Product { Id = 15, Name = "Professional Laptop", Price = 1599.99m, CategoryId = 2, ImageUrl = "/images/Laptop.png", Description = "A business-class laptop designed for productivity. Features a high-performance processor, ample storage, and enhanced security features like fingerprint authentication and facial recognition." },
                new Product { Id = 16, Name = "Smart Speaker", Price = 129.99m, CategoryId = 5, ImageUrl = "/images/mus.png", Description = "An AI-powered smart speaker with built-in voice control, superior sound quality, and smart home integration. Control your music, check the weather, and manage your devices with voice commands." }
            );
        }

        public static async Task SeedAdminUser(IServiceProvider serviceProvider)
        {
            var _accountService = serviceProvider.GetRequiredService<AccountService>();
            var logger = serviceProvider.GetRequiredService<ILogger<AccountService>>();

            try
            {
                await _accountService.InitAdminAccount();
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while seeding the admin user.");
            }
        }
    }
}