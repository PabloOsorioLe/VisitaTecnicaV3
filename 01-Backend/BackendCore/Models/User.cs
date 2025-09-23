namespace BackendCore.Models
{
    public class User
    {
        public int RegID { get; set; } // PK autoincremental
        public string Rut { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public int SystemId { get; set; }
    }
}
