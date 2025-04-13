using E_Commerce_Platform.EF.Models;
using E_Commerce_Platform.Repositories;
using E_Commerce_Platform.ViewModels;
using Hangfire;
using HtmlAgilityPack;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Search;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using MimeKit;

namespace E_Commerce_Platform.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly UserService _userService;
        private readonly EmailRepository _emailRepository;
        private readonly ILogger<EmailService> _logger;
        private readonly ImapClient _imapClient;
        public EmailService(ImapClient imapClient,IConfiguration config, UserService userService, EmailRepository emailRepository, ILogger<EmailService> logger)
        {
            _config = config;
            _userService = userService;
            _emailRepository = emailRepository;
            _logger = logger;
            _imapClient = imapClient;  
        }

        public async Task SendBulkEmailAsync(string subject, string message)
        {
            var users = await _userService.GetAllUsersAsync(false);
            var senderEmail = _config["EmailSettings:SenderEmail"];

            foreach (var user in users)
            {
                if (user.FullName == "Admin")
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var emailMessage = new MimeMessage();
                    emailMessage.From.Add(new MailboxAddress("E-Commerce Platform", senderEmail));
                    emailMessage.To.Add(new MailboxAddress(user.FullName, user.Email));
                    emailMessage.Subject = subject;

                    string emailBody = await FormulateEmail(message, user.FullName, user.Email, true);

                    emailMessage.Body = new TextPart("html")
                    {
                        Text = emailBody
                    };

                    using (var client = new SmtpClient())
                    {
                        await client.ConnectAsync(
                            _config["EmailSettings:SmtpServer"],
                            int.Parse(_config["EmailSettings:SmtpPort"]),
                            SecureSocketOptions.StartTls
                        );

                        await client.AuthenticateAsync(
                            senderEmail,
                            _config["EmailSettings:SenderPassword"]
                        );

                        await client.SendAsync(emailMessage);
                        await client.DisconnectAsync(true);
                    }
                }
            }
        }

        public void ScheduleEmail(DateTime scheduledTime, string subject, string message)
        {
            BackgroundJob.Schedule(() => SendBulkEmailAsync(subject, message), scheduledTime);
        }

        public void ScheduleFetchingEmails(DateTime scheduledTime)
        {
            BackgroundJob.Schedule(() => SyncEmailsToDatabase(), scheduledTime);
        }
        public async Task<string> FormulateEmail(string name, string email, string message, bool advert)
        {
            var model1 = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Contact Form Submission</title>
            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
            <style>
                .email-container {{
                    max-width: 700px;
                    width: 100%;
                    margin: 20px auto;
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 8px;
                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                    font-family: Arial, sans-serif;
                    max-height: 45vh;
                    overflow-y: auto;
                }}
                .header {{
                    background-color: #dc3545;
                    color: white;
                    padding: 15px;
                    text-align: center;
                    border-radius: 8px 8px 0 0;
                    font-size: 20px;
                    font-weight: bold;
                }}
                .content {{
                    padding: 20px;
                    font-size: 16px;
                    line-height: 1.6;
                    color: #333;
                }}
                .message-box {{
                    background-color: #f8f9fa;
                    padding: 15px;
                    border-left: 4px solid #dc3545;
                    margin-top: 10px;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    padding-top: 10px;
                    border-top: 1px solid #ddd;
                    margin-top: 20px;
                }}
            </style>
        </head>
        <body>
             <!-- TEMPLATE_ID: CONTACT_FORM -->
            <div class='email-container'>
                <div class='header'>New Contact Form Submission</div>
                <div class='content'>
                    <p><strong>Name:</strong> {name}</p>
                    <p><strong>Email:</strong> {email}</p>
                    <div class='message-box'>
                        <p><strong>Message:</strong></p>
                        <p>{message}</p>
                    </div>
                </div>
                <div class='footer'>
                    <p>This message was sent from the E-Commerce Platform contact form.</p>
                </div>
            </div>
        </body>
        </html>";

            var model2 = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Special Offer</title>
            <link href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css' rel='stylesheet'>
            <style>
                .email-container {{
                    max-width: 700px;
                    width: 100%;
                    margin: 20px auto;
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 8px;
                    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
                    font-family: Arial, sans-serif;
                    max-height: 45vh;
                    overflow-y: auto;
                }}
                .header {{
                    background-color: #007bff;
                    color: white;
                    padding: 15px;
                    text-align: center;
                    border-radius: 8px 8px 0 0;
                    font-size: 22px;
                    font-weight: bold;
                }}
                .content {{
                    padding: 20px;
                    font-size: 16px;
                    line-height: 1.6;
                    color: #333;
                }}
                .footer {{
                    text-align: center;
                    font-size: 12px;
                    color: #777;
                    padding-top: 10px;
                    border-top: 1px solid #ddd;
                    margin-top: 20px;
                }}
                .btn {{
                    display: block;
                    width: 200px;
                    margin: 20px auto;
                    padding: 10px;
                    background-color: #007bff;
                    color: white;
                    text-align: center;
                    text-decoration: none;
                    border-radius: 5px;
                    font-size: 16px;
                }}
            </style>
        </head>
        <body>
            <!-- TEMPLATE_ID: CONTACT_FORM -->
            <div class='email-container'>
                <div class='header'>Exclusive Offer for You!</div>
                <div class='content'>
                    <p>Hi {name},</p>
                    <p>{message}</p>
                    <a href='https://localhost:7048/Product/ShopPage' class='btn'>View Offer</a>
                </div>
                <div class='footer'>
                    <p>Thank you for shopping with us!</p>
                </div>
            </div>
        </body>
        </html>";

            return advert ? model2 : model1;
        }


        public async Task SendEmailAsync(ContactFormViewModel model, bool isAdminReply, string emailId = null, string resetLink = null)
        {
            var emailMessage = new MimeMessage();
            var adminEmail = _config["EmailSettings:SenderEmail"];

            emailMessage.From.Add(new MailboxAddress("E-Commerce Support", adminEmail));

            if (isAdminReply)
            {
                emailMessage.To.Add(new MailboxAddress(model.Name, model.Email));
                emailMessage.Subject = model.Subject.StartsWith("Re:", StringComparison.OrdinalIgnoreCase)
                    ? model.Subject
                    : "Re: " + model.Subject;
            }
            else if (!string.IsNullOrEmpty(resetLink))
            {
                // Forgot Password Email
                emailMessage.To.Add(new MailboxAddress(model.Name, model.Email));
                emailMessage.Subject = "Password Reset Request";

                model.Message = $@"
                                <p>Hello {model.Name},</p>
                                <p>You requested a password reset. Click the link below to reset your password:</p>
                                <p><a href='{resetLink}' style='color: blue;'>Reset Password</a></p>
                                <p>If you didn't request this, please ignore this email.</p>
                                <p>Regards, <br> E-Commerce Support Team</p>";
            }
            else
            {
                emailMessage.To.Add(new MailboxAddress("Admin", adminEmail));
                emailMessage.Subject = "New Contact Form Submission";

                if (!string.IsNullOrWhiteSpace(model.Email) && model.Email.Contains("@"))
                {
                    emailMessage.ReplyTo.Add(new MailboxAddress(model.Name, model.Email));
                }
            }

            string emailBody = await FormulateEmail(model.Name, model.Email, model.Message, false);

            emailMessage.Body = new TextPart("html") { Text = emailBody };

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(
                    _config["EmailSettings:SmtpServer"],
                    int.Parse(_config["EmailSettings:SmtpPort"]),
                    SecureSocketOptions.StartTls
                );

                await client.AuthenticateAsync(adminEmail, _config["EmailSettings:SenderPassword"]);
                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }

            if (isAdminReply && !string.IsNullOrEmpty(emailId))
            {
                var reply = new EmailReply
                {
                    EmailId = emailId,
                    InnerHTML = emailBody,
                    ReplyBody = model.Message,
                    ReplyDate = DateTime.UtcNow
                };

                await _emailRepository.SaveReplyToDatabase(reply);
            }
        }

        public async Task SyncEmailsToDatabase()
        {
            try
            {
                if (!_imapClient.IsConnected)
                {
                    await _imapClient.ConnectAsync("imap.gmail.com", 993, true);
                    await _imapClient.AuthenticateAsync(
                        _config["EmailSettings:SenderEmail"],
                        _config["EmailSettings:SenderPassword"]
                    );
                }

                var inbox = _imapClient.Inbox;
                await inbox.OpenAsync(FolderAccess.ReadOnly);

                var uids = await inbox.SearchAsync(SearchQuery.All);

                foreach (var uid in uids)
                {
                    var message = await inbox.GetMessageAsync(uid);
                    if (!await _emailRepository.EmailExistsInDatabase(message.MessageId))
                    {
                        await _emailRepository.SaveEmailToDatabase(message);
                    }
                }
                _logger.LogInformation("Email sync completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error syncing emails: {ex.Message}");
            }
        }
        public async Task<EF.Models.Email> GetEmailAsync(string emailId)
        {
            return await _emailRepository.GetEmailAsync(emailId);
        }
        public async Task MarkSeen(EF.Models.Email model)
        {
            model.Seen = true;
            _emailRepository.Update(model);
            await _emailRepository.SaveAsync();
        }
        public async Task<EmailViewModel> LoadEmailPageAsync(int page = 1, int pageSize = 10, string search = "", string sortBy = "")
        {
            IQueryable<EF.Models.Email> query = _emailRepository.GetEmailsQuery();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.Subject.Contains(search) || e.Sender.Contains(search));
            }

            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "Subject" => query.OrderBy(e => e.Subject),
                    "Sender" => query.OrderBy(e => e.Sender),
                    "Date" => query.OrderByDescending(e => e.ReceivedDate),
                    _ => query
                };
            }
            else
            {
                query = query.OrderByDescending(e => e.ReceivedDate);
            }

            int totalEmails = await query.CountAsync();
            List<EmailDTO> pagedEmails = await ApplyPagination(query, page, pageSize)
                .Select(e => new EmailDTO
                {
                    Id = e.Id,
                    Seen = e.Seen == true ? "Read" : "Unread",
                    Sender = e.Sender,
                    Subject = e.Subject,
                    DateReceived = e.ReceivedDate,
                    BodyPreview = e.Body.Substring(0, Math.Min(100, e.Body.Length)) 
                })
                .OrderByDescending(e => e.Seen)
                .ToListAsync();

            return new EmailViewModel
            {
                Emails = pagedEmails,
                TotalPages = CalculateTotalPages(totalEmails, pageSize),
                CurrentPage = page,
                TotalEmails = totalEmails,
                SearchQuery = search,
                SortBy = sortBy
            };
        }

        private IQueryable<EF.Models.Email> ApplyPagination(IQueryable<EF.Models.Email> query, int page, int pageSize)
        {
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        private int CalculateTotalPages(int totalEmails, int pageSize)
        {
            return (int)Math.Ceiling(totalEmails / (double)pageSize);
        }
    }
}