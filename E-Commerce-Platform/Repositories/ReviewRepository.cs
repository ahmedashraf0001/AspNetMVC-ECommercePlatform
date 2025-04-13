using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class ReviewRepository : Repository<Review>
    {
        private AppContextDB _context;

        public ReviewRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _context.reviews.Include(e => e.User).Where(r => r.ProductId == productId).ToListAsync();
        }

        public async Task<List<Review>> GetReviewsByUserIdAsync(string userId)
        {
            return await _context.reviews.Include(e => e.User).Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<IdentityResult> AddAsync(Review rev)
        {
            try
            {
                await _context.reviews.AddAsync(rev);
                await SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Review" });
            }
            return IdentityResult.Success;
        }

        public async Task<int> UpdateLikesAsync(int reviewId, bool increment)
        {
            var model = await GetByIdAsync(reviewId);
            if (model == null) return -1;

            model.Likes += increment ? 1 : (model.Likes > 0 ? -1 : 0);
            await SaveAsync();

            return model.Likes;
        }
    }
}