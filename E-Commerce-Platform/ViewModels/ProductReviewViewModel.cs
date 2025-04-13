using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    public class ProductReviewViewModel
    {
        public Product Product { get; set; }

        public int quantity { get; set; }

        public double avgRating { get; set; }
        public List<Review> Reviews { get; set; }

        public List<Product> RelatedProducts { get; set; }

        public List<int> LikedReviewsIds { get; set; }
    }
}