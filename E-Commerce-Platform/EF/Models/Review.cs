using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Platform.EF.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int Likes { get; set; } = 0;

        [Required]
        [Range(1, 5)]
        public double Rating { get; set; } = 0;

        [Required]
        [MaxLength(500, ErrorMessage = "Review Text name cannot exceed 500 characters.")]
        public string ReviewText { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public ICollection<Like> LikesInfo { get; set; } = new List<Like>();
    }
}