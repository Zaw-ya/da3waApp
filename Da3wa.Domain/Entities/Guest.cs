namespace Da3wa.Domain.Entities
{
    public class Guest : BaseEntity
    {
        public string? FullName { get; set; } 
        public List<string> Tel { get; set; } = [];
        public bool IsAttending { get; set; } = false;
        public int FamilyNumber { get; set; } = 1;
        public string? QrToken { get; set; }
        public DateTime ExpireAt { get; set; }


        public int? EventId { get; set; }
        public virtual Event? Event { get; set; }
    }
}
