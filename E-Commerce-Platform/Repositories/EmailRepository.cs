using E_Commerce_Platform.EF;
using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories.Repo;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Text.RegularExpressions;

namespace E_Commerce_Platform.Repositories
{
    public class EmailRepository: Repository<Email>
    {
        public AppContextDB _context;
        public EmailRepository(AppContextDB context):base(context)
        {
            _context = context;
        }
        public async Task<List<Email>> GetAllEmailsAsync()
        {
            return await _context.emails.ToListAsync();
        }
        public async Task<bool> EmailExistsInDatabase(string emailId)
        {
            return await _context.emails.AnyAsync(e => e.Id == emailId);
        }
        public async Task<Email> GetEmailAsync(string emailId)
        {
            return await _context.emails
                .Include(e => e.Replies)
                .FirstOrDefaultAsync(e => e.Id == emailId);
        }

        public async Task SaveEmailToDatabase(MimeMessage message)
        {
            string body = message.TextBody ?? ExtractTextFromHtml(message.HtmlBody) ?? "[No body content]";
            string senderEmail = message.From.Mailboxes.FirstOrDefault()?.Address ?? "[Unknown Sender]";

            var email = new Email
            {
                Id = message.MessageId,
                Sender = ExtractEmail(body) == "e90650222@gmail.com" ? senderEmail : ExtractEmail(body),
                Subject = message.Subject,
                Body = body,
                InnerHTML = message.HtmlBody,
                ReceivedDate = message.Date.DateTime
            };

            _context.emails.Add(email);
            await _context.SaveChangesAsync();
        }
        public IQueryable<Email> GetEmailsQuery()
        {
            return _context.emails;
        }
        string ExtractEmail(string input)
        {
            var match = Regex.Match(input, @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}");
            return match.Success ? match.Value : "No email found";
        }
        private string ExtractTextFromHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return null;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            string text = doc.DocumentNode.InnerText;
            string filtered = Regex.Replace(text, @"\s{2,}", " ").Trim();
            string cleanedText = filtered.Replace("Contact Form Submission", "").Trim();
            string reallyclean = cleanedText.Replace("New  Name:", "").Trim();
            return reallyclean;
        }
        public async Task SaveReplyToDatabase(EmailReply reply)
        {
            _context.emailReplies.Add(reply);
            await _context.SaveChangesAsync();
        }
    }
}
