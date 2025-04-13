using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    public class ShopViewModel
    {
        public List<ProductViewModel> Products { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalProducts { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }

        public List<Category> Productcategories { get; set; }
    };
}

public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; }
    public bool IsDeleted { get; set; }
}