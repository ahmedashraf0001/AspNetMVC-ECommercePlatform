using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce_Platform.Services
{
    public class CartService
    {
        private readonly CartRepository Repository;
        private readonly ProductService _productService;

        public CartService(CartRepository cartRepository, ProductService productService)
        {
            Repository = cartRepository;
            _productService = productService;
        }

        public async Task ClearCartAsync(string userId)
        {
            await Repository.ClearCartAsync(userId);
        }

        public async Task<IdentityResult> AddToCartAsync(string userId, int productId, int quantity)
        {
            var product = await _productService.GetProductByIdAsync(productId, withDeletes: false);
            if (product == null || product.Stock <= 0)
            {
                return IdentityResult.Failed(new IdentityError { Description = "This product is currently out of stock." });
            }

            var existingItem = await Repository.GetCartItemAsync(userId, productId);
            if (existingItem != null)
            {
                int newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Not enough stock available for this product." });
                }

                existingItem.Quantity = newQuantity;
                await Repository.UpdateQuantityAsync(existingItem.Id, existingItem.Quantity);
            }
            else
            {
                if (quantity > product.Stock)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Not enough stock available for this product." });
                }

                var newCartItem = new Cart { UserId = userId, ProductId = productId, Quantity = quantity };
                await Repository.AddToCartAsync(newCartItem);
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateQuantityAsync(string userId, int productId, int newQuantity)
        {
            var cartItem = await Repository.GetCartItemAsync(userId, productId);
            if (cartItem == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Item not found in cart." });
            }
            if (newQuantity <= 0)
            {
                return await RemoveFromCartAsync(userId, productId);
            }

            await Repository.UpdateQuantityAsync(cartItem.Id, newQuantity);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveFromCartAsync(string userId, int productId)
        {
            var cartItem = await Repository.GetCartItemAsync(userId, productId);
            if (cartItem == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Item not found in cart." });
            }

            await Repository.RemoveFromCartAsync(cartItem.Id);
            return IdentityResult.Success;
        }

        public async Task<List<Cart>> GetCartItemsByUserIdAsync(string userId)
        {
            return await Repository.GetCartItemsByUserIdAsync(userId);
        }

        public async Task<CartSummaryViewModel> LoadCartAsync(string userId)
        {
            var cart = await Repository.GetCartItemsByUserIdAsync(userId);
            CartSummaryViewModel model = new CartSummaryViewModel();

            model.Products = cart.Select(item => new CartProductViewModel
            {
                ProductId = item.Product.Id,
                ProductName = item.Product.Name,
                ProductDescription = item.Product.Description,
                Category = item.Product.Category?.Name ?? "Unknown",
                Quantity = item.Quantity,
                Price = item.Product.Price.ToString("N0"),
                ProductImage = item.Product.ImageUrl
            }).ToList();
            model.TotalCost = cart.Sum(item => item.Quantity * item.Product.Price).ToString("N0");

            return model;
        }
    }
}