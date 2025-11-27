namespace Application.Dtos
{
    public record DashboardStatisticsDto(
        int TotalIncidents,
        int ActiveIncidents,
        int ResolvedIncidents,
        int TotalResponders,
        int AvailableResponders,
        int TotalAgencies,
        int TotalUsers,
        DashboardTrendsDto Trends
    );

    public record DashboardTrendsDto(
        double IncidentsChangePercent,
        double RespondersChangePercent,
        double UsersChangePercent
    );
}
