using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum UserRole
    {
        [Display(Name = "SuperAdmin")]
        SuperAdmin = 1,
        [Display(Name = "AgencyAdmin")]
        AgencyAdmin,
        [Display(Name = "Responder")]
        Responder,
        [Display(Name = "Victim")]
        Victim,
        Unknown
    }

}
