using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace E_Commerce_Platform.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly Services.CheckoutService _checkoutService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(Services.CheckoutService checkoutService, ILogger<CheckoutController> logger)
        {
            _checkoutService = checkoutService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ProcessCheckout(int fees)
        {
            var CurrentId = User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;
            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to access checkout.");
                return RedirectToAction("Login", "Account");
            }
            var url = await _checkoutService.ProcessPayment(CurrentId, fees);
            if (url.Item1.Succeeded)
            {
                return Redirect(url.Item2);
            }
            else
            {
                _logger.LogError("Checkout failed for user {UserId}. Errors: {Errors}", CurrentId, string.Join(", ", url.Item1.Errors.Select(e => e.Description)));

                TempData["ErrorMessage"] = "Payment processing failed. Please try again.";
                return RedirectToAction("CartPage", "Cart");
            }
        }

        public async Task<IActionResult> Success(int fees, string session_id)
        {
            var CurrentId = User.Claims.FirstOrDefault(e => e.Type == ClaimTypes.NameIdentifier)?.Value;

            if (CurrentId == null)
            {
                _logger.LogWarning("Unauthorized attempt to access checkout.");
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("User {UserId} is attempting checkout with fees {Fees}.", CurrentId, fees);

            var service = new SessionService();
            Session session = await service.GetAsync(session_id);

            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

            var paymentMethod = paymentIntent.PaymentMethod;
            string cardBrand = string.Empty;
            bool isDebit = false;

            if (paymentMethod != null)
            {
                var cardDetails = paymentMethod.Card;

                if (cardDetails != null)
                {
                    cardBrand = cardDetails.Brand;
                    isDebit = cardDetails.Funding == "debit";
                    _logger.LogInformation("Payment completed with card type: {CardBrand}, Is Debit: {IsDebit}", cardBrand, isDebit);
                }
            }

            var paymentType = isDebit ? "debitcard" : "creditcard";
            var result = await _checkoutService.ProcessCheckout(CurrentId, paymentType, fees, session.PaymentIntentId);

            if (result.Succeeded)
            {
                _logger.LogInformation("Payment successful, Session ID: {SessionId}", session_id);
                _logger.LogInformation("Checkout successful for user {UserId}.", CurrentId);
                return View("successCheckout");
            }

            _logger.LogError("Checkout failed for user {UserId}. Errors: {Errors}", CurrentId, string.Join(", ", result.Errors.Select(e => e.Description)));
            await _checkoutService._checkoutMediatorService.RefundPaymentAsync(session.PaymentIntentId);
            return View("FailedCheckout", new FailedCheckoutModel {  ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description)) });
        }

        public IActionResult Cancel()
        {
            _logger.LogWarning("Payment was canceled.");
            return RedirectToAction("CartPage", "Cart");
        }
    }
}