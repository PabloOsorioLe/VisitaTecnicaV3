namespace BackendCore.Models
{
    public class Role
    {
        public int RegID { get; set; } // PK autoincremental
        public string RoleName { get; set; } = null!;
        public string? Description { get; set; }
        public int SystemId { get; set; }
    }
}
