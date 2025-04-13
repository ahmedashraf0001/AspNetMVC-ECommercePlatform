namespace E_Commerce_Platform.ViewModels
{
    public class CartSummaryViewModel
    {
        public List<CartProductViewModel> Products { get; set; } = new List<CartProductViewModel>();

        public string TotalCost { get; set; }
    }

    public class CartProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public string Category { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public string ProductImage { get; set; }
    }
}