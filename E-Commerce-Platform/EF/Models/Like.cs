using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Platform.EF.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int ReviewId { get; set; }
        public string UserId { get; set; }

        [ForeignKey("ReviewId")]
        public Review Review { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}