using Da3wa.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Da3wa.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Gender? Gender { get; set; }
        [MaxLength(20)]
        public string FirstName { get; set; } =null!;
        [MaxLength(20)]
        public string LastName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string PrimaryContactNo { get; set; } = null!;
        public string SecondaryContactNo { get; set; } = null!;
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public bool? IsActive { get; set; }
        public bool? Approved { get; set; } = false;
        public int? Role { get; set; }
        public string? DeviceId { get; set; }

        public int? CityId { get; set; }

        [ForeignKey("CityId")]
        public virtual City? City { get; set; }
    }
}
