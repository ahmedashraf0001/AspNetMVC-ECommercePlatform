using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class OrderRepository : Repository<Order>
    {
        private AppContextDB _context;

        public OrderRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders.Include(e => e.User).Include(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().ToListAsync();
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders.Include(e => e.User).Include(e => e.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<Order> GetOrderAsync(int orderId)
        {
            return await _context.Orders.Include(e => e.User).Include(o => o.OrderDetails).ThenInclude(e => e.Product).IgnoreQueryFilters().FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public List<Order> GetOrdersByDate(DateTime date)
        {
            return _context.Orders
                .Where(o => o.OrderDate.Date == date.Date)
                .ToList();
        }

        public async Task<decimal> GetRevenueByDate(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .SumAsync(o => o.TotalCost);

            return orders;
        }

        public async Task<List<Order>> GetRecentOrders(int n)
        {
            var orders = await _context.Orders
                                          .OrderByDescending(e => e.OrderDate)
                                          .Take(n).ToListAsync();
            return orders;
        }

        public async Task<Dictionary<string, int>> GetOrderStatusCount()
        {
            var orders = await _context.Orders
                                            .GroupBy(o => o.Status)
                                            .ToDictionaryAsync(g => g.Key.ToString(), g => g.Count());
            return orders;
        }

        public async Task<int> GetOrdersCount(DateTime date)
        {
            return await _context.Orders.CountAsync(o => o.OrderDate.Date == date.Date);
        }

        public async Task<int> GetPendingTicketsCount()
        {
            var orders = await _context.Orders
                                          .CountAsync(o => o.Status == OrderStatus.Pending);
            return orders;
        }
        public async Task<IdentityResult> AddOrderAsync(Order order)
        {
            try
            {
                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Error Adding Order" });
            }
            return IdentityResult.Success;
        }

        public IQueryable<Order> GetOrderQuery()
        {
            return _context.Orders.AsQueryable();
        }

        internal IQueryable<Transactions> Include(Func<object, object> value)
        {
            throw new NotImplementedException();
        }
    }
}