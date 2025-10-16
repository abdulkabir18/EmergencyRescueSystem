using Application.Common.Dtos;

namespace Application.Interfaces.External
{
    public interface IEmailService
    {
        Task<Result<string>> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);
        Task<Result<string>> SendEmailAsync(IEnumerable<string> toEmail, string subject, string body, bool isHtml = true);
    }
}
