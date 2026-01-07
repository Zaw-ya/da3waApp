namespace Da3wa.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MessageTitle { get; set; }
        public string? SystemTitle { get; set; }
        public string? HallName { get; set; }
        public string? Bridegroom { get; set; }
        public string? Bride { get; set; }
        public string? Venue { get; set; }
        public string? Location { get; set; }
        public string? Type { get; set; }
        public string? ImagePath { get; set; }
        public string? TemplateFilePath { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public DateTime? AttendTime { get; set; }
        public DateTime? LeaveTime { get; set; }



        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        public int? CityId { get; set; }
        public virtual City? City { get; set; }

        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }
        public virtual ICollection<Guest>? Guests { get; set; }

    }
}
