using Da3wa.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Da3wa.Application.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = null!;

        [Required]
        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; } = null!;

        [Required]
        [Display(Name = "Primary Contact Number")]
        public string PrimaryContactNo { get; set; } = null!;

        [Required]
        [Display(Name = "Secondary Contact Number")]
        public string SecondaryContactNo { get; set; } = null!;

        [Required]
        [Display(Name = "City")]
        public int CityId { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; } = null!;

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Approved")]
        public bool Approved { get; set; } = false;
    }
}


