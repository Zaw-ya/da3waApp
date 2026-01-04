using System.ComponentModel.DataAnnotations;

namespace Da3wa.Domain.Entities
{
    public class City : BaseEntity
    {

        [Required]
        public string CityName { get; set; } = null!;
        public int? CountryId { get; set; }
        public virtual Country? Country { get; set; }
        public virtual ICollection<ApplicationUser>? ApplicationUsers { get; set; }
        public virtual ICollection<Event>? Events { get; set; }
    }
}
