using E_Commerce_Platform.Controllers;
using Microsoft.AspNetCore.Identity;
using Stripe;

namespace E_Commerce_Platform.Services
{
    public class CheckoutMediatorService
    {
        private readonly ILogger<CheckoutController> _logger;
        public CheckoutMediatorService(ILogger<CheckoutController> logger)
        {
            _logger = logger;
        }
        public async Task<IdentityResult> RefundPaymentAsync(string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                _logger.LogError("Invalid PaymentIntent ID.");
                return IdentityResult.Failed(new IdentityError { Description = "Invalid PaymentIntent ID." });
            }
            try
            {
                var refundService = new RefundService();
                var refund = await refundService.CreateAsync(new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId,
                    Reason = "requested_by_customer"
                });

                return IdentityResult.Success;
            }
            catch (StripeException ex)
            {
                _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
                return IdentityResult.Failed(new IdentityError { Description = $"Stripe refund failed: {ex.Message}" });
            }
        }
    }
}
