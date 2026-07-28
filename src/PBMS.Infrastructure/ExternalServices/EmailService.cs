using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using PBMS.Application.Auth.Interfaces;

namespace PBMS.Infrastructure.ExternalServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var brevoApiKey = _configuration["Brevo:ApiKey"] ?? _configuration["BREVO_API_KEY"];
            
            // Nếu có Brevo API Key (và không phải placeholder mặc định), ưu tiên gửi qua Brevo HTTP REST API v3 (Port 443 HTTPS - Chống bị chặn cổng 100%)
            if (!string.IsNullOrWhiteSpace(brevoApiKey) && 
                !brevoApiKey.Contains("YOUR_BREVO_API_KEY", StringComparison.OrdinalIgnoreCase))
            {
                await SendViaBrevoRestApiAsync(toEmail, subject, body, brevoApiKey.Trim());
                return;
            }

            // Mặc định hoặc fallback: Gửi qua SMTP Relay (MailKit)
            await SendViaSmtpAsync(toEmail, subject, body);
        }

        /// <summary>
        /// Gửi email qua Brevo HTTP REST API v3 (Port 443 HTTPS - Không bao giờ bị chặn cổng)
        /// </summary>
        private async Task SendViaBrevoRestApiAsync(string toEmail, string subject, string body, string apiKey)
        {
            var displayName = _configuration["Smtp:DisplayName"] ?? "NexPark System";
            var fromEmail = _configuration["Smtp:FromEmail"] ?? "noreply@nexpark.id.vn";

            _logger.LogInformation("Sending email to {ToEmail} via Brevo HTTP REST API v3 (Sender: {FromEmail})", toEmail, fromEmail);

            var payload = new
            {
                sender = new
                {
                    name = displayName,
                    email = fromEmail
                },
                to = new[]
                {
                    new { email = toEmail }
                },
                subject = subject,
                htmlContent = body
            };

            var jsonContent = JsonSerializer.Serialize(payload);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("api-key", apiKey);
            httpRequest.Headers.Add("accept", "application/json");

            var client = _httpClientFactory.CreateClient("BrevoApiClient");
            var response = await client.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo REST API failed with status code {StatusCode}. Error: {ErrorBody}", response.StatusCode, errorBody);
                throw new InvalidOperationException($"Brevo REST API email delivery failed: {response.StatusCode} - {errorBody}");
            }

            _logger.LogInformation("Email sent successfully to {ToEmail} via Brevo HTTP REST API v3", toEmail);
        }

        /// <summary>
        /// Gửi email qua giao thức SMTP Relay (MailKit)
        /// </summary>
        private async Task SendViaSmtpAsync(string toEmail, string subject, string body)
        {
            var host = _configuration["Smtp:Host"] ?? "smtp-relay.brevo.com";
            var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_configuration["Smtp:EnableSsl"] ?? "true");
            var rawUsername = _configuration["Smtp:Username"] ?? throw new InvalidOperationException("SMTP Username is not configured.");
            var rawPassword = _configuration["Smtp:Password"] ?? throw new InvalidOperationException("SMTP Password is not configured.");
            var displayName = _configuration["Smtp:DisplayName"] ?? "NexPark System";
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
                _logger.LogInformation("Email sent successfully to {ToEmail} via SMTP", toEmail);

                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FAILED to send email via SMTP to {ToEmail}. Error: {ErrorMessage}", toEmail, ex.Message);
                throw;
            }
        }
    }
}
