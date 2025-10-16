using Application.Interfaces.External;
using Domain.Common.Templates;
using Domain.Enums;
using Domain.Events;
using MediatR;

namespace Application.Features.Users.EventHandlers
{
    public class UserRegisteredEventHandler : INotificationHandler<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;
        private readonly IVerificationService _verificationService;

        public UserRegisteredEventHandler(IEmailService emailService, IVerificationService verificationService)
        {
            _emailService = emailService;
            _verificationService = verificationService;
        }
        public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
        {
            switch (notification.Role)
            {
                case UserRole.Victim:
                    var code = await _verificationService.GenerateCodeAsync(notification.UserId);

                    await _emailService.SendEmailAsync(
                        notification.Email,
                        EmailTemplates.GetVerificationSubject(),
                        EmailTemplates.GetVerificationBody(code)
                    );
                    break;

                case UserRole.AgencyAdmin:
                case UserRole.Responder:
                    await _emailService.SendEmailAsync(
                        notification.Email,
                        EmailTemplates.GetAccountCreatedSubject(),
                        EmailTemplates.GetAccountCreatedBody(notification.Role.ToString())
                    );
                    break;

                // optional: log or ignore unexpected roles
                default:
                    // e.g. SuperAdmin doesn’t need emails
                    break;
            }
        }
    }
    
}
