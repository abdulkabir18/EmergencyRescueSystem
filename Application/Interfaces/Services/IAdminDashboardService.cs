using Application.Common.Dtos;
using Application.Dtos;

namespace Application.Interfaces.Services
{
    public interface IAdminDashboardService
    {
        Task<Result<DashboardStatisticsDto>> GetDashboardStatisticsAsync();
    }
}