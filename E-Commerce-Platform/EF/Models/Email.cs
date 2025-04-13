namespace E_Commerce_Platform.EF.Models
{
    public class Email
    {
        public string Id { get; set; }
        public string Sender { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string InnerHTML { get; set; }
        public bool Seen { get; set; } = false;
        public DateTime ReceivedDate { get; set; }
        public List<EmailReply> Replies { get; set; } = new();

    }

}
