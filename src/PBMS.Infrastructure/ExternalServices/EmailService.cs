using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Smtp:Host"] ?? "smtp.gmail.com";
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
            var rawUsername = _configuration["Smtp:Username"] ?? throw new InvalidOperationException("SMTP Username is not configured.");
            var rawPassword = _configuration["Smtp:Password"] ?? throw new InvalidOperationException("SMTP Password is not configured.");
            var displayName = _configuration["Smtp:DisplayName"] ?? "PBMS Team";
            var fromEmail = _configuration["Smtp:FromEmail"] ?? rawUsername;

            var username = rawUsername.Trim().Trim('"');
            var password = rawPassword.Replace(" ", "").Trim().Trim('"');

            _logger.LogInformation("Attempting to send email via SMTP host {Host}:{Port} for user {Username} (Sender: {FromEmail})", host, port, username, fromEmail);

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(displayName, fromEmail.Trim().Trim('"')));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = new TextPart(TextFormat.Html) { Text = body };

                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var socketOptions = port switch
                {
                    465 => SecureSocketOptions.SslOnConnect,
                    587 => SecureSocketOptions.StartTls,
                    _ => enableSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None
                };

                await client.ConnectAsync(host, port, socketOptions);
                _logger.LogInformation("SMTP connected successfully to {Host}:{Port}", host, port);

                await client.AuthenticateAsync(username, password);
                _logger.LogInformation("SMTP authenticated successfully for user {Username}", username);

                await client.SendAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED to send email to {ToEmail}. Error: {ErrorMessage}", toEmail, ex.Message);
                throw;
            }
        }
    }
}
