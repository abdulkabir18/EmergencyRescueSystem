using Domain.Enums;

namespace Application.Interfaces.External
{
    public interface IAIService
    {
        Task<IncidentAIResult> AnalyzeIncidentMediaAsync(string mediaUrl);

    }

    public record IncidentAIResult(string Title, IncidentType Type, double Confidence, string? Evidence = null, string? Recommended_Action = null);
}