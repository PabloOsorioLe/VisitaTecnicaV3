namespace BackendCore.Models
{
    public class System
    {
        public int RegID { get; set; } // PK autoincremental
        public string SystemName { get; set; } = null!;
        public string? Description { get; set; }
    }
}
