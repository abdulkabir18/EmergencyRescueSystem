using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum IncidentStatus
    {
        [Display(Name = "Pending")]
        Pending = 1,
        [Display(Name = "Reported")]
        Reported,
        [Display(Name = "Inprogress")]
        InProgress,
        [Display(Name = "Resolved")]
        Resolved,
        [Display(Name = "Escalated")]
        Escalated,
        [Display(Name = "Cancelled")]
        Cancelled
    }
}