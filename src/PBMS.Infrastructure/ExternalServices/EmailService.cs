using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using PBMS.Application.Auth.Interfaces;
using System;
using System.Threading.Tasks;

namespace PBMS.Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
            var rawUsername = _configuration["Smtp:Username"] ?? throw new InvalidOperationException("SMTP Username is not configured.");
            var rawPassword = _configuration["Smtp:Password"] ?? throw new InvalidOperationException("SMTP Password is not configured.");
            var displayName = _configuration["Smtp:DisplayName"] ?? "PBMS Team";

            // Tự động làm sạch Username và Password (loại bỏ dấu cách và dấu ngoặc kép thừa từ Render/Local)
            var username = rawUsername.Trim().Trim('"');
            var password = rawPassword.Replace(" ", "").Trim().Trim('"');

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(displayName, username));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = body };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            // Tự động chọn chế độ mã hóa SSL/TLS phù hợp với Linux Container & Render
            var socketOptions = port switch
            {
                465 => SecureSocketOptions.SslOnConnect,
                587 => SecureSocketOptions.StartTls,
                _ => enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None
            };

            await client.ConnectAsync(host, port, socketOptions);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
