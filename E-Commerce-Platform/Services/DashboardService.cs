namespace E_Commerce_Platform.Services
{
    public class DashboardService
    {
        private readonly UserService _userService;
        private readonly OrderService _orderService;
        private readonly ProductService _productService;

        public DashboardService(UserService userService, OrderService orderService, ProductService productService)
        {
            _userService = userService;
            _orderService = orderService;
            _productService = productService;
        }

        public List<string> GenerateLastNthMonths(int n)
        {
            List<string> months = new List<string>();
            for (int i = 0; i < n; i++)
            {
                months.Add(DateTime.Now.AddMonths(-i).ToString("MMM"));
            }
            return months;
        }

        public async Task<List<decimal>> GenerateLastNthMonthsRevenueAsync(int n)
        {
            List<decimal> revenue = new List<decimal>();

            for (int i = 0; i < n; i++)
            {
                DateTime startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-i);
                DateTime endDate = startDate.AddMonths(1);

                revenue.Add(await _orderService.GetRevenueByDate(startDate, endDate));
            }

            return revenue;
        }

        public List<string> GenerateLastNthDays(int n)
        {
            List<string> days = new List<string>();
            for (int i = 0; i < n; i++)
            {
                days.Add(DateTime.Now.AddDays(-i).ToString("ddd"));
            }
            return days;
        }

        public async Task<List<int>> GenerateLastNthDaysOrderCountAsync(int n)
        {
            List<int> orders = new List<int>();
            for (int i = 0; i < n; i++)
            {
                DateTime date = DateTime.Now.AddDays(-i);
                orders.Add(await _orderService.GetOrdersCount(date));
            }
            return orders;
        }

        public async Task<AdminDashboardViewModel> PrepareDashboard(int n)
        {
            AdminDashboardViewModel model = new AdminDashboardViewModel()
            {
                TotalUsers = await _userService.GetTotalUsers(),
                OrdersToday = await _orderService.GetOrdersCount(DateTime.Now),
                MonthlyRevenue = await _orderService.GetRevenueByDate(DateTime.Now.AddMonths(-1), DateTime.Now),
                PendingIssues = await _orderService.GetPendingTicketsCount(),
                MonthlyRevenueListLabels = GenerateLastNthMonths(n),
                MonthlyRevenueList = await GenerateLastNthMonthsRevenueAsync(n),
                DailyOrdersDateLabels = GenerateLastNthDays(n),
                DailyOrders = await GenerateLastNthDaysOrderCountAsync(n),
                RecentOrders = await _orderService.GetRecentOrders(3),
                OrderStatusLabels = (await _orderService.GetOrderStatusCount()).Keys.ToList(),
                OrderStatusCounts = (await _orderService.GetOrderStatusCount()).Values.ToList(),
                TopProducts = await _productService.GetTopProductsAsync(4)
            };
            return model;
        }
    }
}