using E_Commerce_Platform.Controllers;
using E_Commerce_Platform.EF.Models;
using Microsoft.AspNetCore.Identity;
using Stripe;
using Stripe.Checkout;

namespace E_Commerce_Platform.Services
{
    public class CheckoutService
    {
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;
        private readonly TransactionService _transactionService;
        private readonly OrderdetailService _orderdetailService;
        public  readonly CheckoutMediatorService _checkoutMediatorService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutService(CheckoutMediatorService checkoutMediatorService,ProductService productService, CartService cartService, OrderService orderService, TransactionService transactionService, OrderdetailService orderdetailService, ILogger<CheckoutController> logger, IConfiguration configuration)
        {
            _productService = productService;
            _cartService = cartService;
            _orderService = orderService;
            _transactionService = transactionService;
            _orderdetailService = orderdetailService;
            _logger = logger;
            _configuration = configuration;
            _checkoutMediatorService = checkoutMediatorService;
        }

        public async Task<(IdentityResult, string)> ProcessPayment(string userId, int fees)
        {
            List<Cart> cartItems = await _cartService.GetCartItemsByUserIdAsync(userId);
            if (cartItems == null || !cartItems.Any())
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Cart is empty." }), null);
            }
            _logger.LogInformation("User {UserId} is initiating checkout.", userId);
            var domain = _configuration["Stripe:Domain"];
            var list = cartItems.Select(e => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = e.Product.Name,
                    },
                    UnitAmount = (long)(e.Product.Price * 100),
                },
                Quantity = e.Quantity,
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = list,
                Mode = "payment",
                SuccessUrl = $"{domain}/checkout/success?fees={fees}&session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/checkout/cancel",
            };
            var service = new SessionService();
            try
            {
                Session session = await service.CreateAsync(options);
                _logger.LogInformation("Stripe Checkout Session created for User {UserId}, Session ID: {SessionId}", userId, session.Id);
                return (IdentityResult.Success, session.Url);
            }
            catch (StripeException ex)
            {
                _logger.LogError("Stripe error occurred: {Message}", ex.Message);
                return (IdentityResult.Failed(new IdentityError { Description = "Payment processing failed. Please try again." }), null);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
                return (IdentityResult.Failed(new IdentityError { Description = "An unexpected error occurred. Please try again later." }), null);
            }
        }
        public async Task<IdentityResult> ProcessCheckout(string userId, string Paymentmethod, int fees, string paymentIntentId)
        {
            try
            {
                var cart = await _cartService.GetCartItemsByUserIdAsync(userId);
                if (cart == null || !cart.Any())
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Cart is empty." });
                }
                string msg = "";
                List<string> outOfStockProducts = new();

                foreach (var item in cart)
                {
                    var product = await _productService.GetProductByIdAsync(item.ProductId, withDeletes: false);

                    if (product == null)
                    {
                        outOfStockProducts.Add($"Product with ID '{item.ProductId}' is unavailable.");
                    }
                    else if (product.Stock < item.Quantity)
                    {
                        outOfStockProducts.Add($"'{product.Name}' has only {product.Stock} left, but you requested {item.Quantity}.");
                    }
                    else
                    {
                        product.Stock -= item.Quantity;
                        await _productService.UpdateProductAsync(product, null, product.ImageUrl);
                    }
                }

                if (outOfStockProducts.Any())
                {
                    msg = "Some items in your cart cannot be processed:\n" + string.Join("\n", outOfStockProducts) + " Payment Refunded!";

                    return IdentityResult.Failed(new IdentityError { Description = msg });
                }
                Order newOrderRecord = new Order
                {
                    UserId = userId,
                    PaymentIntentId = paymentIntentId,
                    OrderDate = DateTime.Now,
                    Status = OrderStatus.Pending,
                    TotalCost = cart.Sum(c => c.Product.Price * c.Quantity) + fees
                };
                var result = await _orderService.AddOrderAsync(newOrderRecord);
                if (result.Succeeded)
                {
                    var orderdetailList = new List<OrderDetail>();
                    foreach (var item in cart)
                    {
                        var existingorderdetail = await _orderdetailService.GetOrderDetailsByOrderAndProductAsync(newOrderRecord.Id, item.ProductId);
                        if (existingorderdetail != null)
                        {
                            existingorderdetail.Quantity += item.Quantity;
                            await _orderdetailService.UpdateOrderDetailAsync(existingorderdetail);
                        }
                        else
                        {
                            orderdetailList.Add(new OrderDetail
                            {
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                OrderId = newOrderRecord.Id
                            });
                        }
                    }
                    if (orderdetailList.Any())
                    {
                        var orderDetailsResult = await _orderdetailService.AddOrderDetailList(orderdetailList);
                        if (!orderDetailsResult.Succeeded)
                        {
                            await _orderService.DeleteOrderAsync(newOrderRecord.Id);
                            return IdentityResult.Failed(new IdentityError { Description = "Error adding order details." });
                        }
                    }
                    EF.Models.PaymentMethod Payment = Paymentmethod.ToLower() switch
                    {
                        "creditcard" => EF.Models.PaymentMethod.CreditCard,
                        "debitcard" => EF.Models.PaymentMethod.DebitCard,
                        _ => EF.Models.PaymentMethod.CreditCard
                    };
                    var transaction = new Transactions
                    {
                        UserId = userId,
                        OrderId = newOrderRecord.Id,
                        TotalCost = newOrderRecord.TotalCost,
                        paymentMethod = Payment,
                        transactionStatus = TransStatus.Pending,
                        TransactionDate = DateTime.Now
                    };
                    var transactionResult = await _transactionService.AddTransactionAsync(transaction);
                    if (transactionResult.Succeeded)
                    {
                        await _cartService.ClearCartAsync(userId);
                    }
                    else
                    {
                        await _orderService.DeleteOrderAsync(newOrderRecord.Id);
                        await _orderdetailService.RemoveOrderDetailAsync(newOrderRecord.Id);
                        return IdentityResult.Failed(new IdentityError { Description = "Error adding transaction." });
                    }
                }
                else
                {
                    return IdentityResult.Failed(new IdentityError { Description = "Error adding order." });
                }
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Unexpected error: {ex.Message}" });
            }
        }
    }
}