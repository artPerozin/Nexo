using Microsoft.EntityFrameworkCore;
using Nexo.Models;

namespace Nexo.Contexts
{
    public class NexoContext(DbContextOptions<NexoContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permissao> Permissoes => Set<Permissao>();
        public DbSet<RolePermissao> RolePermissoes => Set<RolePermissao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Chave composta N:N
            modelBuilder.Entity<RolePermissao>()
                .HasKey(rp => new { rp.RoleId, rp.PermissaoId });

            modelBuilder.Entity<RolePermissao>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissoes)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermissao>()
                .HasOne(rp => rp.Permissao)
                .WithMany(p => p.RolePermissoes)
                .HasForeignKey(rp => rp.PermissaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Evita nome duplicado de role
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Nome)
                .IsUnique();

            // Evita nome duplicado de permissão
            modelBuilder.Entity<Permissao>()
                .HasIndex(p => p.Nome)
                .IsUnique();
        }
    }
}
