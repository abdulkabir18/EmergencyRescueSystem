using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public record Media(string FileUrl, MediaType MediaType);
}