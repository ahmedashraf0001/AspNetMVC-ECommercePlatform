using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Services
{
    public class OrderService
    {
        private readonly OrderRepository _repository;
        private readonly CheckoutMediatorService _checkoutMediatorService;

        public OrderService(OrderRepository repository, CheckoutMediatorService checkoutService)
        {
            _repository = repository;
            _checkoutMediatorService = checkoutService;
        }
        public async Task<OrderPageViewModel> LoadOrderPageAsync(
           int page = 1,
           int pageSize = 12,
           string search = "",
           string sortBy = "")
        {
            IQueryable<Order> query = _repository.GetOrderQuery().Include(e => e.User);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Id.ToString().Contains(search)
                                    || e.User.FullName.ToLower().Contains(search)
                                    || e.TotalCost.ToString().Contains(search)
                                    || e.OrderDate.ToString().Contains(search)
                                    || e.Status.ToString().ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "FullName" => query.OrderBy(u => u.User.FullName),
                    "TotalCost" => query.OrderBy(u => u.TotalCost),
                    "Id" => query.OrderBy(u => u.Id),
                    "OrderDate" => query.OrderBy(u => u.OrderDate),
                    "Status" => query.OrderBy(u => u.Status),
                    _ => query.OrderByDescending(u => u.OrderDate) 
                };
            }

            int totalOrders = await query.CountAsync();
            List<Order> pagedOrders = await ApplyPagination(query, page, pageSize).ToListAsync();

            return new OrderPageViewModel
            {
                Orders = pagedOrders,
                TotalPages = CalculateTotalPages(totalOrders, pageSize),
                CurrentPage = page,
                TotalOrders = totalOrders,
                SearchQuery = search,
                SortBy = sortBy
            };
        }

        public IQueryable<Order> ApplyPagination(IQueryable<Order> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private int CalculateTotalPages(int totalOrders, int pageSize)
        {
            return (int)Math.Ceiling(totalOrders / (double)pageSize);
        }

        public async Task<Order> GetOrderAsync(int orderId)
        {
            return await _repository.GetOrderAsync(orderId);
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _repository.GetAllOrdersAsync();
        }

        public async Task<decimal> GetRevenueByDate(DateTime startDate, DateTime endDate)
        {
            var orders = await _repository.GetRevenueByDate(startDate, endDate);
            return orders;
        }

        public async Task<List<Order>> GetRecentOrders(int n)
        {
            var orders = await _repository.GetRecentOrders(n);
            return orders;
        }

        public async Task<Dictionary<string, int>> GetOrderStatusCount()
        {
            var orders = await _repository.GetOrderStatusCount();
            return orders;
        }

        public async Task<int> GetOrdersCount(DateTime date)
        {
            return await _repository.GetOrdersCount(date);
        }

        public async Task<int> GetPendingTicketsCount()
        {
            var orders = await _repository.GetPendingTicketsCount();
            return orders;
        }

        public async Task<IdentityResult> DeleteOrderAsync(int orderId)
        {
            var model = await _repository.GetOrderAsync(orderId);

            if (model == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Order not found." });
            }
            if (model.Status == OrderStatus.Shipped || model.Status == OrderStatus.Delivered)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Order Cannot be cancelled out because it's status is {model.Status.ToString()}." });
            }
            foreach (var item in model.OrderDetails)
            {
                item.Product.Stock += item.Quantity;
            }
            try
            {
                model.Status = OrderStatus.Cancelled;
                _repository.Update(model);
                await _repository.SaveAsync();
                var result = await _checkoutMediatorService.RefundPaymentAsync(model.PaymentIntentId);
                if (!result.Succeeded)
                {
                    throw new Exception();
                }
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Deleting Order" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> AddOrderAsync(Order order)
        {
            try
            {
                var result = await _repository.AddOrderAsync(order);
                if (!result.Succeeded)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Order" });
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChangeStatusAsync(OrderStatus status, int orderId)
        {
            try
            {
                var model = await _repository.GetByIdAsync(orderId);
                model.Status = status;
                _repository.Update(model);
                await _repository.SaveAsync();
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Error Updating Status!" });
            }
            return IdentityResult.Success;
        }
    }
}