using Application.Common.Dtos;
using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly IIncidentRepository _incidentRepository;
        private readonly IResponderRepository _responderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IDashboardTrendsService _dashboardTrendsService;

        public AdminDashboardService(IIncidentRepository incidentRepository, IResponderRepository responderRepository, IUserRepository userRepository, IAgencyRepository agencyRepository, IDashboardTrendsService dashboardTrendsService)
        {
            _agencyRepository = agencyRepository;
            _responderRepository = responderRepository;
            _userRepository = userRepository;
            _incidentRepository = incidentRepository;
            _dashboardTrendsService = dashboardTrendsService;
        }

        public async Task<Result<DashboardStatisticsDto>> GetDashboardStatisticsAsync()
        {
            var totalIncidents = await _incidentRepository.CountAsync();
            var activeIncidents = await _incidentRepository.CountAsync(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Cancelled && i.Status != IncidentStatus.Invalid);
            var resolvedIncidents = await _incidentRepository.CountAsync(i => i.Status == IncidentStatus.Resolved);

            var totalResponders = await _responderRepository.CountAsync();
            var availableResponders = await _responderRepository.CountAsync(r => r.Status == ResponderStatus.Available);

            var totalAgencies = await _agencyRepository.CountAsync();
            var totalUsers = await _userRepository.CountAsync();

            var trends = await _dashboardTrendsService.CalculateTrendsAsync();

            return Result<DashboardStatisticsDto>.Success(new DashboardStatisticsDto(
                totalIncidents, activeIncidents, resolvedIncidents, totalResponders, availableResponders, totalAgencies, totalUsers, trends
                ), "Statistics retrieved successfully");
        }
    }
}