using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Services
{
    public class ReviewService
    {
        private ReviewRepository Repository;
        private LikeService likesService;

        public ReviewService(ReviewRepository _reviewRepository, LikeService _likeService)
        {
            Repository = _reviewRepository;
            likesService = _likeService;
        }

        public async Task<List<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await Repository.GetReviewsByProductIdAsync(productId);
        }

        public async Task AddAsync(Review rev)
        {
            await Repository.AddAsync(rev);
        }

        public async Task<List<int>> GetLikedReviewsByUserAsync(int productId, string userId)
        {
            var userLikes = likesService.GetLikesQuery()
                                        .Where(like => like.UserId == userId &&
                                                       like.Review.ProductId == productId)
                                        .Select(like => like.ReviewId);

            return await userLikes.ToListAsync();
        }

        public async Task<int> GetLikesCount(int reviewId)
        {
            return (await Repository.GetByIdAsync(reviewId)).Likes;
        }

        public async Task<int> ToggleLikeAsync(int reviewId, string userId)
        {
            try
            {
                var existingLike = await likesService.GetLikeByReviewIdAndUserIdAsync(reviewId, userId);

                if (existingLike != null)
                {
                    var removeResult = await likesService.RemoveLikeAsync(reviewId, userId);
                    if (!removeResult.Succeeded) return -1;

                    return await Repository.UpdateLikesAsync(reviewId, false);
                }
                else
                {
                    var addResult = await likesService.AddLikeAsync(reviewId, userId);
                    if (!addResult.Succeeded) return -1;

                    return await Repository.UpdateLikesAsync(reviewId, true);
                }
            }
            catch
            {
                return -1;
            }
        }
    }
}