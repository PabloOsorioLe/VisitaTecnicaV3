using BackendCore.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendCore.Data
{
    public class BackendCoreContext : DbContext
    {
        public BackendCoreContext(DbContextOptions<BackendCoreContext> options) : base(options)
        {
        }

        public DbSet<Visita> Visitas { get; set; } = default!;
        public DbSet<Persona> Personas { get; set; } = default!;
        public DbSet<Reunion> Reuniones { get; set; } = default!;

        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Role> Roles { get; set; } = default!;
        public DbSet<UserRole> UserRoles { get; set; } = default!;
        public DbSet<Permission> Permissions { get; set; } = default!;
        public DbSet<RolePermission> RolePermissions { get; set; } = default!;
        public DbSet<BackendCore.Models.System> Systems { get; set; } = default!;
        public DbSet<UserToken> UserTokens { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasKey(u => u.RegID);

            modelBuilder.Entity<Role>()
                .HasKey(r => r.RegID);

            modelBuilder.Entity<UserRole>()
                .HasKey(ur => ur.RegID);

            modelBuilder.Entity<Permission>()
                .HasKey(p => p.RegID);

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => rp.RegID);

            modelBuilder.Entity<BackendCore.Models.System>()
                .HasKey(s => s.RegID);

            // Puedes agregar más configuraciones de relaciones si las necesitas aquí
        }
    }
}
