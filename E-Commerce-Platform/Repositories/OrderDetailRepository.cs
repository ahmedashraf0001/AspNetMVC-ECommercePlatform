using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Platform.Repositories
{
    public class OrderDetailRepository : Repository<OrderDetail>
    {
        private AppContextDB _context;

        public OrderDetailRepository(AppContextDB context) : base(context)
        {
            _context = context;
        }

        public async Task<OrderDetail> GetOrderDetailsByOrderAndProductAsync(int orderId, int productId)
        {
            return await _context.orderDetails.FirstOrDefaultAsync(od => od.OrderId == orderId && od.ProductId == productId);
        }

        public async Task<List<OrderDetail>> GetOrderDetailsByOrderIdAsync(int orderId)
        {
            return await _context.orderDetails.Where(od => od.OrderId == orderId).ToListAsync();
        }

        public async Task AddOrderDetailAsync(OrderDetail orderDetail)
        {
            await _context.orderDetails.AddAsync(orderDetail);
            await SaveAsync();
        }

        public async Task AddBatchAsync(List<OrderDetail> batch)
        {
            await _context.orderDetails.AddRangeAsync(batch);
            await SaveAsync();
        }

        public async Task RemoveOrderDetailAsync(int orderDetailId)
        {
            var model = await GetByIdAsync(orderDetailId);
            _context.orderDetails.Remove(model);
            await SaveAsync();
        }

        public async Task UpdateOrderDetailAsync(OrderDetail orderDetail)
        {
            _context.orderDetails.Update(orderDetail);
            await SaveAsync();
        }
    }
}