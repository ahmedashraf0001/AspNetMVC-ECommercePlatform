using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int OrdersToday { get; set; }

    public decimal MonthlyRevenue { get; set; }

    public int PendingIssues { get; set; }

    public List<string> MonthlyRevenueListLabels { get; set; } 
    public List<decimal> MonthlyRevenueList { get; set; } 

    public List<string> DailyOrdersDateLabels { get; set; } 
    public List<int> DailyOrders { get; set; }

    public List<Order> RecentOrders { get; set; }
    public List<string> OrderStatusLabels { get; set; }
    public List<int> OrderStatusCounts { get; set; }

    public List<TopProductViewModel> TopProducts { get; set; }
}