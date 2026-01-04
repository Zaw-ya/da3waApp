using Da3wa.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Da3wa.Application.DTOs
{
    public class UpdateUserDto
    {
        [Required]
        public string Id { get; set; } = null!;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [MaxLength(20)]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [MaxLength(20)]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Primary Contact Number")]
        public string? PrimaryContactNo { get; set; }

        [Display(Name = "Secondary Contact Number")]
        public string? SecondaryContactNo { get; set; }

        [Display(Name = "City")]
        public int? CityId { get; set; }

        [Display(Name = "Role")]
        public string? Role { get; set; }

        [Display(Name = "Is Active")]
        public bool? IsActive { get; set; }

        [Display(Name = "Approved")]
        public bool? Approved { get; set; }
    }
}

