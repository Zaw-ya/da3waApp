namespace Da3wa.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = null!;

        public virtual ICollection<Event>? Events { get; set; }
    }
}
