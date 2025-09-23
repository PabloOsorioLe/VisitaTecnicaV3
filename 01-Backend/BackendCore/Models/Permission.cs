namespace BackendCore.Models
{
    public class Permission
    {
        public int RegID { get; set; } // PK autoincremental
        public string PermissionName { get; set; } = null!;
        public string? Description { get; set; }
        public int SystemId { get; set; }
    }
}
