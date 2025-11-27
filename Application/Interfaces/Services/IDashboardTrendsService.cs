using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IDashboardTrendsService
    {
        Task<DashboardTrendsDto> CalculateTrendsAsync();
    }
}
