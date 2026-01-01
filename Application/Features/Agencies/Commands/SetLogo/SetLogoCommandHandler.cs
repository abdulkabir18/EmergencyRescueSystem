using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Commands.SetLogo
{
    public class SetLogoCommandHandler : IRequestHandler<SetLogoCommand, Result<Unit>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICacheService _cacheService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStorageService _storageService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SetLogoCommandHandler> _logger;

        public SetLogoCommandHandler(ICacheService cacheService, IAgencyRepository agencyRepository, ICurrentUserService currentUserService, IStorageService storageService, IUnitOfWork unitOfWork, ILogger<SetLogoCommandHandler> logger)
        {
            _agencyRepository = agencyRepository;
            _cacheService = cacheService;
            _currentUserService = currentUserService;
            _storageService = storageService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result<Unit>> Handle(SetLogoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Logo == null)
                {
                    _logger.LogWarning("SetLogoCommand received with a null model.");
                    return Result<Unit>.Failure("SetLogoCommand received with a null model.");
                }
                else if (request.Logo.Length >= 5 * 1024 * 1024)
                {
                    _logger.LogWarning("SetLogoCommand received with a logo bigger than 5 MB");
                    return Result<Unit>.Failure("SetLogoCommand received with a logo bigger than 5 MB");
                }

                Guid currentUserId = _currentUserService.UserId;
                if (currentUserId == Guid.Empty)
                {
                    _logger.LogWarning("Unauthenticated user attempted to set logo.");
                    return Result<Unit>.Failure("User is not authenticated.");
                }

                var agency = await _agencyRepository.GetAsync(a => a.AgencyAdminId == currentUserId && !a.IsDeleted);
                if (agency == null)
                {
                    _logger.LogWarning("Agency not found for the current user.");
                    return Result<Unit>.Failure("Agency not found for the current user.");
                }

                using var stream = request.Logo.OpenReadStream();
                string imageUrl = await _storageService.UploadAsync(stream, request.Logo.FileName, request.Logo.ContentType, "naijarescue/agency-logos");

                agency.SetLogo(imageUrl);

                await _agencyRepository.UpdateAsync(agency);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Logo updated successfully for agency {AgencyId}.", agency.Id);
                return Result<Unit>.Success(Unit.Value, "Logo updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while setting logo.");
                return Result<Unit>.Failure("An error occurred while setting logo.");
            }
        }
    }
}