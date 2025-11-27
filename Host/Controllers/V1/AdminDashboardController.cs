using Application.Common.Dtos;
using Application.Dtos;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IAdminDashboardService _adminDashboardService;

        public AdminDashboardController(IAdminDashboardService adminDashboardService)
        {
            _adminDashboardService = adminDashboardService;
        }

        [Authorize]
        [HttpGet("dashboard-statistics")]
        public async Task<ActionResult<Result<DashboardStatisticsDto>>> GetDashboardStatistics()
        {
            var result = await _adminDashboardService.GetDashboardStatisticsAsync();

            return Ok(result);
        }
    }
}