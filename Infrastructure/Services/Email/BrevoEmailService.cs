using Application.Common.Dtos;
using Application.Interfaces.External;
using brevo_csharp.Api;
using brevo_csharp.Client;
using brevo_csharp.Model;
using Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Email
{
    public class BrevoEmailService : IEmailService
    {
        private readonly TransactionalEmailsApi _transactionalEmailsApi;
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<BrevoEmailService> _logger;
        public BrevoEmailService(TransactionalEmailsApi transactionalEmailsApi, IOptions<EmailSettings> email, ILogger<BrevoEmailService> logger)
        {
            _transactionalEmailsApi = transactionalEmailsApi;
            _emailSettings = email.Value;
            _logger = logger;
        }
        public async Task<Result<string>> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var sender = new SendSmtpEmailSender(_emailSettings.FromName, _emailSettings.FromEmail);
                var toList = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(toEmail) };

                var email = new SendSmtpEmail(
                    sender: sender,
                    to: toList,
                    subject: subject,
                    htmlContent: isHtml ? body : null,
                    textContent: !isHtml ? body : null
                );

                var response = await _transactionalEmailsApi.SendTransacEmailAsync(email);
                if (string.IsNullOrEmpty(response.MessageId))
                    return Result<string>.Failure("Failed to send email.");

                _logger.LogInformation("Email sent successfully to {ToEmail}.Subject: {Subject} MessageId: {MessageId}", toEmail, subject, response.MessageId);
                return Result<string>.Success($"Email sent successfully. MessageId: {response.MessageId}");
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "Brevo API error while sending email.");
                return Result<string>.Failure($"API Error sending email: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email.");
                return Result<string>.Failure($"Error sending email: {ex.Message}");
            }
        }

        public async Task<Result<string>> SendEmailAsync(IEnumerable<string> toEmail, string subject, string body, bool isHtml = true)
        {
            try
            {
                var sender = new SendSmtpEmailSender(_emailSettings.FromName, _emailSettings.FromEmail);
                var toList = toEmail.Select(email => new SendSmtpEmailTo(email)).ToList();

                var email = new SendSmtpEmail(
                    sender: sender,
                    to: toList,
                    subject: subject,
                    htmlContent: isHtml ? body : null,
                    textContent: !isHtml ? body : null
                );

                var response = await _transactionalEmailsApi.SendTransacEmailAsync(email);
                if (string.IsNullOrEmpty(response.MessageId))
                    return Result<string>.Failure("Failed to send email.");

                _logger.LogInformation("Email sent successfully to {ToEmails}. Subject: {Subject}, MessageId: {MessageId}", string.Join(", ", toEmail), subject, response.MessageId);
                return Result<string>.Success($"Email sent successfully. MessageId: {response.MessageId}");
            }
            catch (ApiException apiEx)
            {
                _logger.LogError(apiEx, "Brevo API error while sending email.");
                return Result<string>.Failure($"API Error sending email: {apiEx.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while sending email.");
                return Result<string>.Failure($"Error sending email: {ex.Message}");
            }
        }
    }
}
