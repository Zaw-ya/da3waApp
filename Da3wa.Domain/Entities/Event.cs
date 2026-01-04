namespace Da3wa.Domain.Entities
{
    public class Event : BaseEntity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Type { get; set; }

        public int? CityId { get; set; }
        public virtual City? City { get; set; }

        public int? CategoryId { get; set; }
        public virtual Category? Category { get; set; }

    }
}
