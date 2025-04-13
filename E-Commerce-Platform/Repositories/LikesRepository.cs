using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class LikesRepository : Repository<Like>
    {
        public AppContextDB _context;

        public LikesRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Like>> GetLikesByReviewIdAsync(int reviewId)
        {
            return await _context.likes.Include(e => e.Review).Include(e => e.User).Where(r => r.ReviewId == reviewId).ToListAsync();
        }

        public async Task<List<Like>> GetLikesByUserIdAsync(string userId)
        {
            return await _context.likes.Include(e => e.Review).Include(e => e.User).Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task<Like> GetLikeByReviewIdAndUserIdAsync(int reviewId, string userId)
        {
            return await _context.likes.FirstOrDefaultAsync(e => e.ReviewId == reviewId && e.UserId == userId);
        }

        public async Task<IdentityResult> AddLikeAsync(int reviewId, string userId)
        {
            try
            {
                var model = new Like
                {
                    ReviewId = reviewId,
                    UserId = userId
                };
                await AddAsync(model);
                await SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Like" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> RemoveLikeAsync(int reviewId, string userId)
        {
            try
            {
                var like = await GetLikeByReviewIdAndUserIdAsync(reviewId, userId);
                if (like == null)
                    return IdentityResult.Failed(new IdentityError { Description = "Like not found" });

                Delete(like);
                await SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Removing Like" });
            }
            return IdentityResult.Success;
        }

        public IQueryable<Like> GetLikesQuery()
        {
            return _context.likes.AsQueryable();
        }
    }
}