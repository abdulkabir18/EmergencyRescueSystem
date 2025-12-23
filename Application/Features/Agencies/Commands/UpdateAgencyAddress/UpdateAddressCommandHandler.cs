using Application.Common.Dtos;
using Application.Interfaces.CurrentUser;
using Application.Interfaces.External;
using Application.Interfaces.Repositories;
using Application.Interfaces.UnitOfWork;
using Domain.ValueObject;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Agencies.Commands.UpdateAgencyAddress
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result<Unit>>
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateAddressCommandHandler> _logger;
        private readonly ICacheService _cacheService;

        public UpdateAddressCommandHandler(IAgencyRepository agencyRepository, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ILogger<UpdateAddressCommandHandler> logger, ICacheService cacheService)
        {
            _agencyRepository = agencyRepository;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<Result<Unit>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            Guid currentUserId = _currentUserService.UserId;
            if (currentUserId == Guid.Empty)
            {
                _logger.LogWarning("Unauthenticated user attempted to update address.");
                return Result<Unit>.Failure("User is not authenticated.");
            }


            var agency = await _agencyRepository.GetAsync(a => a.AgencyAdminId == currentUserId && !a.IsDeleted);
            if (agency == null)
            {
                _logger.LogWarning("Agency not found for the current user.");
                return Result<Unit>.Failure("Agency not found for the current user.");
            }

            agency.SetAddress(new Address(request.Address.Street!, request.Address.City!, request.Address.State!, request.Address.LGA!, request.Address.Country!, request.Address.PostalCode!));

            await _agencyRepository.UpdateAsync(agency);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Address updated successfully for agency admin {UserId}.", _currentUserService.UserId);
            return Result<Unit>.Success(Unit.Value, "Address updated successfully.");
        }
    }
}