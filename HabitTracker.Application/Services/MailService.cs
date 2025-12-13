    using HabitTracker.Application.Interfaces;
    using HabitTracker.Application.Settings;
    using MimeKit;
    using MailKit.Net.Smtp;
    using MailKit.Security;
    using Microsoft.Extensions.Options; 
    namespace HabitTracker.Application.Services

    {
        public class MailService : IMailService
        {
            private readonly MailSettings _mailSettings;

            public MailService(IOptions<MailSettings> mailSettings)
            {
                _mailSettings = mailSettings.Value;
            }

            public async Task SendEmailAsync(string tomail, string subject, string body)
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("habit-tracker", _mailSettings.From));
                emailMessage.To.Add(new MailboxAddress("Recipient", tomail));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = body
                };
                emailMessage.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {

                    await smtpClient.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.SslOnConnect);
                    await smtpClient.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
                    await smtpClient.SendAsync(emailMessage);
                    await smtpClient.DisconnectAsync(true);
                }
            }
    
        }
    }
