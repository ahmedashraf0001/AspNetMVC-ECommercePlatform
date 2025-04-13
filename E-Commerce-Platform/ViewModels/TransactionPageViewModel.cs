using E_Commerce_Platform.EF.Models;

namespace E_Commerce_Platform.ViewModels
{
    public class TransactionPageViewModel
    {
        public List<Transactions> Transactions { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalTransactions { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
    }
}
