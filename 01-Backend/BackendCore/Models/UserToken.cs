using System;
using System.ComponentModel.DataAnnotations;

namespace BackendCore.Models
{
    public class UserToken
    {
        [Key]
        public int RegID { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public string? DeviceInfo { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; } = false;
    }
}
