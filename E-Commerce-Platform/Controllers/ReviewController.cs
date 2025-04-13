using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ReviewService _reviewService;
        private readonly ILogger<ReviewController> _logger;

        public ReviewController(ReviewService reviewService, ILogger<ReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateReview(Review rev, int productId)
        {
            try
            {
                _logger.LogInformation("User {UserId} is attempting to add a review for Product ID: {ProductId}.", User.FindFirstValue(ClaimTypes.NameIdentifier), productId);
                await _reviewService.AddAsync(rev);
                _logger.LogInformation("Review added successfully for Product ID: {ProductId}.", productId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding a review for Product ID: {ProductId}.", productId);
                return Content("An error occurred: " + ex.Message);
            }
            return RedirectToAction("ProductPage", "Product", new { productId = productId });
        }

        public async Task<IActionResult> UpdateLikes(int reviewId)
        {
            var currentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} is toggling like for Review ID: {ReviewId}.", currentId, reviewId);

            var likes = await _reviewService.ToggleLikeAsync(reviewId, currentId);

            if (likes == -1)
            {
                _logger.LogWarning("Failed to toggle like for Review ID: {ReviewId} by User ID: {UserId}.", reviewId, currentId);
                return BadRequest();
            }

            _logger.LogInformation("Successfully updated likes for Review ID: {ReviewId}. New like count: {Likes}.", reviewId, likes);

            return Json(new { Likes = likes });
        }
    }
}