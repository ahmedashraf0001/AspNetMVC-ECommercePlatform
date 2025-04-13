using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    public class OrderPageViewModel
    {
        public List<Order> Orders { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalOrders { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
    }
}
