using System.ComponentModel.DataAnnotations;

namespace Da3wa.Domain.Entities
{
    public class Country : BaseEntity
    {
        [Required]
        public string CountryName { get; set; } = null!;
        public virtual ICollection<City>? Cities { get; set; }
    }
}
