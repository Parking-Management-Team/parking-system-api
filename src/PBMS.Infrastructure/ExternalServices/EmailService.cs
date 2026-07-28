using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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

            if (string.IsNullOrWhiteSpace(brevoApiKey) ||
                brevoApiKey.Contains("YOUR_BREVO_API_KEY", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Brevo API Key is not configured. Please set Brevo:ApiKey in appsettings.json.");
            }

            await SendViaBrevoRestApiAsync(toEmail, subject, body, brevoApiKey.Trim());
        }

        /// <summary>
        /// Gửi email thuần túy qua Brevo HTTP REST API v3 (Port 443 HTTPS)
        /// </summary>
        private async Task SendViaBrevoRestApiAsync(string toEmail, string subject, string body, string apiKey)
        {
            var displayName = _configuration["Smtp:DisplayName"] ?? _configuration["Brevo:DisplayName"] ?? "NexPark System";
            var fromEmail = _configuration["Smtp:FromEmail"] ?? _configuration["Brevo:FromEmail"] ?? "noreply@nexpark.id.vn";

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
    }
}
