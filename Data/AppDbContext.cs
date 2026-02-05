using Microsoft.EntityFrameworkCore;
using Nexo.Models;

namespace Nexo.Data
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permissao> Permissoes { get; set; }
        public DbSet<RolePermissao> RolePermissoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // Usuario
            // ======================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");

                entity.HasKey(u => u.Id);

                entity.Property(u => u.Nome)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(u => u.Email)
                    .IsUnique();

                entity.Property(u => u.Senha)
                    .IsRequired();

                entity.Property(u => u.Ativo)
                    .IsRequired()
                    .HasDefaultValue(true);

                entity.Property(u => u.DataCriacao)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // 🔥 RELAÇÃO CORRETA Usuario -> Role
                entity.HasOne(u => u.Role)
                    .WithMany(r => r.Usuarios)
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ======================
            // Role
            // ======================
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");

                entity.HasKey(r => r.Id);

                entity.Property(r => r.Nome)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(r => r.Nome)
                    .IsUnique();
            });

            // ======================
            // Permissao
            // ======================
            modelBuilder.Entity<Permissao>(entity =>
            {
                entity.ToTable("permissoes");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(p => p.Nome)
                    .IsUnique();
            });

            // ======================
            // RolePermissao (JOIN TABLE)
            // ======================
            modelBuilder.Entity<RolePermissao>(entity =>
            {
                entity.ToTable("role_permissoes");

                entity.HasKey(rp => rp.Id);

                entity.HasIndex(rp => new { rp.RoleId, rp.PermissaoId })
                    .IsUnique();

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissoes)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permissao)
                    .WithMany(p => p.RolePermissoes)
                    .HasForeignKey(rp => rp.PermissaoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ======================
            // SEEDS
            // ======================
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Nome = "Admin", Descricao = "Administrador do sistema com acesso total" },
                new Role { Id = 2, Nome = "Gerente", Descricao = "Gerente com permissões de gestão" },
                new Role { Id = 3, Nome = "Usuario", Descricao = "Usuário padrão do sistema" }
            );

            modelBuilder.Entity<Permissao>().HasData(
                new Permissao { Id = 1, Nome = "usuarios.listar", Descricao = "Listar usuários" },
                new Permissao { Id = 2, Nome = "usuarios.criar", Descricao = "Criar usuários" },
                new Permissao { Id = 3, Nome = "usuarios.editar", Descricao = "Editar usuários" },
                new Permissao { Id = 4, Nome = "usuarios.excluir", Descricao = "Excluir usuários" },
                new Permissao { Id = 5, Nome = "relatorios.visualizar", Descricao = "Visualizar relatórios" },
                new Permissao { Id = 6, Nome = "relatorios.gerar", Descricao = "Gerar relatórios" },
                new Permissao { Id = 7, Nome = "dashboard.admin", Descricao = "Dashboard Admin" },
                new Permissao { Id = 8, Nome = "dashboard.gerente", Descricao = "Dashboard Gerente" },
                new Permissao { Id = 9, Nome = "dashboard.usuario", Descricao = "Dashboard Usuário" }
            );

            modelBuilder.Entity<RolePermissao>().HasData(
                new RolePermissao { Id = 1, RoleId = 1, PermissaoId = 1 },
                new RolePermissao { Id = 2, RoleId = 1, PermissaoId = 2 },
                new RolePermissao { Id = 3, RoleId = 1, PermissaoId = 3 },
                new RolePermissao { Id = 4, RoleId = 1, PermissaoId = 4 },
                new RolePermissao { Id = 5, RoleId = 1, PermissaoId = 5 },
                new RolePermissao { Id = 6, RoleId = 1, PermissaoId = 6 },
                new RolePermissao { Id = 7, RoleId = 1, PermissaoId = 7 },

                new RolePermissao { Id = 8, RoleId = 2, PermissaoId = 1 },
                new RolePermissao { Id = 9, RoleId = 2, PermissaoId = 5 },
                new RolePermissao { Id = 10, RoleId = 2, PermissaoId = 6 },
                new RolePermissao { Id = 11, RoleId = 2, PermissaoId = 8 },

                new RolePermissao { Id = 12, RoleId = 3, PermissaoId = 9 }
            );
        }
    }
}