namespace BackendCore.Models
{
    public class RolePermission
    {
        public int RegID { get; set; } // PK autoincremental
        public int RoleId { get; set; }
        public int PermissionId { get; set; }
    }
}
