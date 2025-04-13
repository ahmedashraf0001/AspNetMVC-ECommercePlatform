using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Platform.EF.Models
{
    public enum OrderStatus
    { Pending, Shipped, Delivered, Cancelled }

    public class Order
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PaymentIntentId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a positive value!")]
        public decimal TotalCost { get; set; } // OrderDetails.Sum(od => od.Quantity * od.Product.Price)

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public Transactions? Transaction { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}