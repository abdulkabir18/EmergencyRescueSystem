using Application.Dtos;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Enums;

namespace Application.Services
{
    public class DashboardTrendsService : IDashboardTrendsService
    {
        private readonly IUserRepository _userRepository;
        private readonly IIncidentRepository _incidentRepository;
        private readonly IResponderRepository _responderRepository;

        public DashboardTrendsService(IUserRepository userRepository, IIncidentRepository incidentRepository, IResponderRepository responderRepository)
        {
            _incidentRepository = incidentRepository;
            _userRepository = userRepository;
            _responderRepository = responderRepository;
        }

        public async Task<DashboardTrendsDto> CalculateTrendsAsync()
        {
            var startOfThisWeek = StartOfWeek(DateTime.UtcNow, DayOfWeek.Monday);
            var startOfLastWeek = startOfThisWeek.AddDays(-7);

            int currentWeekIncidents = await _incidentRepository.CountAsync(i => i.CreatedAt >= startOfThisWeek);
            int previousWeekIncidents = await _incidentRepository.CountAsync(i => i.CreatedAt >= startOfLastWeek && i.CreatedAt < startOfThisWeek);
            double incidentsChange = CalculatePercentChange(currentWeekIncidents, previousWeekIncidents);

            int currentWeekAvailable = await _responderRepository.CountAsync(r => r.Status == ResponderStatus.Available && r.UpdatedAt >= startOfThisWeek);
            int previousWeekAvailable = await _responderRepository.CountAsync(r => r.Status == ResponderStatus.Available && r.UpdatedAt >= startOfLastWeek && r.UpdatedAt < startOfThisWeek);
            double respondersChange = CalculatePercentChange(currentWeekAvailable, previousWeekAvailable);

            var currentWeekUsers = await _userRepository.CountAsync(u => u.CreatedAt >= startOfThisWeek);
            var previousWeekUsers = await _userRepository.CountAsync(u => u.CreatedAt >= startOfLastWeek && u.CreatedAt < startOfThisWeek);
            double usersChange = CalculatePercentChange(currentWeekUsers, previousWeekUsers);

            return new DashboardTrendsDto(incidentsChange, respondersChange, usersChange);
        }

        private static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }

        private static double CalculatePercentChange(int current, int previous)
        {
            if (previous == 0) return current > 0 ? 100 : 0; 
            return ((double)(current - previous) / previous) * 100;
        }
    }
}