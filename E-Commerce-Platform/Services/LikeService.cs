using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce_Platform.Services
{
    public class LikeService
    {
        private readonly LikesRepository Repository;

        public LikeService(LikesRepository _likesRepository)
        {
            Repository = _likesRepository;
        }

        public async Task<List<Like>> GetLikesByUserIdAsync(string userId)
        {
            return await Repository.GetLikesByUserIdAsync(userId);
        }

        public async Task<Like> GetLikeByReviewIdAndUserIdAsync(int reviewId, string userId)
        {
            return await Repository.GetLikeByReviewIdAndUserIdAsync(reviewId, userId);
        }

        public async Task<IdentityResult> AddLikeAsync(int reviewId, string userId)
        {
            return await Repository.AddLikeAsync(reviewId, userId);
        }

        public async Task<List<Like>> GetLikesByReviewIdAsync(int reviewId)
        {
            return await Repository.GetLikesByReviewIdAsync(reviewId);
        }

        public async Task<IdentityResult> RemoveLikeAsync(int reviewId, string userId)
        {
            return await Repository.RemoveLikeAsync(reviewId, userId);
        }

        public IQueryable<Like> GetLikesQuery()
        {
            return Repository.GetLikesQuery();
        }
    }
}