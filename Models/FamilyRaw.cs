namespace NCMIS.Models
{
    public class FamilyRaw
    {
        [key]
        public int Id { get; set; }

        
        public Guid guid { get; set; }
        public int UserId { get; set; }                  // Who submitted the family
        public string JsonData { get; set; }             // Full raw family JSON
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
