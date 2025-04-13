using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    internal class ProductListViewModel
    {
        public List<Product> Products { get; set; }
        public int TotalPages { get; set; }
        public int TotalProducts { get; set; }
    }
}