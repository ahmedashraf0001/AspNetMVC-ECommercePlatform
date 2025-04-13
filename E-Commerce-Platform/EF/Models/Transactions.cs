using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Platform.EF.Models
{
    public enum PaymentMethod
    { CreditCard, DebitCard }

    public enum TransStatus
    {
        Pending,        // Payment initiated but not completed
        Completed,      // Payment successfully processed
        Failed,         // Payment attempt failed
        Refunded,       // Payment refunded to the user
        Chargeback      // Disputed transaction, refunded by the payment provider
    }

    public class Transactions
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Total cost must be a positive value!")]
        public decimal TotalCost { get; set; }

        [Required]
        public PaymentMethod paymentMethod { get; set; }

        [Required]
        public TransStatus transactionStatus { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}