using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using Microsoft.AspNetCore.Identity;

namespace E_Commerce_Platform.Services
{
    public class OrderdetailService
    {
        private readonly OrderDetailRepository _orderDetailRepository;

        public OrderdetailService(OrderDetailRepository orderDetailRepository)
        {
            _orderDetailRepository = orderDetailRepository;
        }

        public async Task<IdentityResult> AddOrderDetailList(List<OrderDetail> orders)
        {
            try
            {
                await _orderDetailRepository.AddBatchAsync(orders);
                await _orderDetailRepository.SaveAsync();
            }
            catch
            {
                return IdentityResult.Failed(new IdentityError { Description = "Order not found." });
            }
            return IdentityResult.Success;
        }

        public async Task<OrderDetail> GetOrderDetailsByOrderAndProductAsync(int orderId, int productId)
        {
            return await _orderDetailRepository.GetOrderDetailsByOrderAndProductAsync(orderId, productId);
        }

        public async Task UpdateOrderDetailAsync(OrderDetail orderDetail)
        {
            await _orderDetailRepository.UpdateOrderDetailAsync(orderDetail);
        }

        public async Task RemoveOrderDetailAsync(int orderDetailId)
        {
            await _orderDetailRepository.RemoveOrderDetailAsync(orderDetailId);
        }
    }
}