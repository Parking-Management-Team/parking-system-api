using System.Threading.Tasks;

namespace PBMS.Application.Auth.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
