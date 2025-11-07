namespace Application.Features.Agencies.Dtos
{
    public record GetAllAgenciesRequest(int PageNumber = 1, int PageSize = 10);
}