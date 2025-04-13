using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class CartRepository : Repository<Cart>
    {
        private AppContextDB _cntx;

        public CartRepository(AppContextDB context) : base(context)
        {
            _cntx = context;
        }

        public async Task<List<Cart>> GetCartItemsByUserIdAsync(string userId)
        {
            return await _cntx.carts.Include(e => e.User).Include(e => e.Product).ThenInclude(e => e.Category).Where(e => e.UserId == userId).ToListAsync();
        }

        public async Task<Cart> GetCartItemAsync(string userId, int productId)
        {
            return await _cntx.carts.Include(e => e.User).Include(e => e.Product).ThenInclude(e => e.Category).FirstOrDefaultAsync(e => e.UserId == userId && e.ProductId == productId);
        }

        public async Task AddToCartAsync(Cart cartItem)
        {
            await _cntx.AddAsync(cartItem);
            await SaveAsync();
        }

        public async Task RemoveFromCartAsync(int cartItemId)
        {
            var model = await GetByIdAsync(cartItemId);
            _cntx.carts.Remove(model);
            await SaveAsync();
        }

        public async Task UpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            var model = await GetByIdAsync(cartItemId);
            model.Quantity = newQuantity;
            await SaveAsync();
        }

        public async Task ClearCartAsync(string userId)
        {
            _cntx.carts.RemoveRange(await GetCartItemsByUserIdAsync(userId));
            await SaveAsync();
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var cartItems = await GetCartItemsByUserIdAsync(userId);
            return cartItems.Sum(e => e.Quantity * e.Product.Price);
        }
    }
}