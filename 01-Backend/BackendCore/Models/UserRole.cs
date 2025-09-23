namespace BackendCore.Models
{
    public class UserRole
    {
        public int RegID { get; set; } // PK autoincremental
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
