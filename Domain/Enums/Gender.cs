using System.ComponentModel.DataAnnotations;

namespace Domain.Enums
{
    public enum Gender
    {
        [Display(Name = "Male")]
        Male = 1,
        [Display(Name = "Female")]
        Female
    }
}
