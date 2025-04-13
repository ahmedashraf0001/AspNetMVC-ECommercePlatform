namespace E_Commerce_Platform.EF.Models
{
    public class EmailReply
    {
        public int Id { get; set; }
        public string EmailId { get; set; }  
        public string ReplyBody { get; set; }
        public DateTime ReplyDate { get; set; }
        public string InnerHTML { get; set; }
        public Email Email { get; set; }
    }

}
