namespace E_Commerce_Platform.ViewModels
{
    public class EmailViewModel
    {
        public List<EmailDTO> Emails { get; set; } = new();
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int TotalEmails { get; set; }
        public string SearchQuery { get; set; }
        public string SortBy { get; set; }
    }
    public class EmailDTO
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }

        public string Seen { get; set; }
        public DateTime DateReceived { get; set; }
        public string BodyPreview { get; set; }
    }
}
