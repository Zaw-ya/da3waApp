using Da3wa.Domain.Enums;

namespace Da3wa.Application.DTOs
{
    public class UserDto
    {
        public string Id { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}".Trim();
        public Gender? Gender { get; set; }
        public string? Address { get; set; }
        public string? PrimaryContactNo { get; set; }
        public string? SecondaryContactNo { get; set; }
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public string? Role { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool? IsActive { get; set; }
        public bool? Approved { get; set; }
        public DateTime? CreatedOn { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
    }
}

