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
        public DbSet<Projeto> Projetos { get; set; }
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
            // RolePermissao
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
            // Projeto
            // ======================
            modelBuilder.Entity<Projeto>(entity =>
            {
                entity.ToTable("projetos");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Nome)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(p => p.Status)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(p => p.Responsavel)
                    .WithMany()
                    .HasForeignKey(p => p.ResponsavelId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // ======================
            // SEEDS
            // ======================
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario { Id = 1, Nome = "Admin", Email = "admin@gmail.com", Senha = "admin123", RoleId = 1 }
            );

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Nome = "Admin", Descricao = "Administrador Total" },
                new Role { Id = 2, Nome = "Vendedor", Descricao = "Acesso a vendas e projetos" },
                new Role { Id = 3, Nome = "Financeiro", Descricao = "Acesso ao financeiro" }
            );

            modelBuilder.Entity<Permissao>().HasData(
                new Permissao { Id = 1, Nome = "dashboard.visualizar", Descricao = "Visualizar Dashboard" },
                
                new Permissao { Id = 2, Nome = "projetos.listar", Descricao = "Listar Projetos" },
                new Permissao { Id = 3, Nome = "projetos.criar", Descricao = "Criar Projetos" },
                new Permissao { Id = 4, Nome = "projetos.editar", Descricao = "Editar Projetos" },
                new Permissao { Id = 5, Nome = "projetos.excluir", Descricao = "Excluir Projetos" },
                
                new Permissao { Id = 6, Nome = "vendas.listar", Descricao = "Listar Vendas" },
                new Permissao { Id = 7, Nome = "vendas.criar", Descricao = "Criar Vendas" },
                new Permissao { Id = 8, Nome = "vendas.editar", Descricao = "Editar Vendas" },
                new Permissao { Id = 9, Nome = "vendas.excluir", Descricao = "Excluir Vendas" },
                
                new Permissao { Id = 10, Nome = "financeiro.visualizar", Descricao = "Visualizar Financeiro" },
                new Permissao { Id = 11, Nome = "financeiro.criar", Descricao = "Criar Transações" },
                new Permissao { Id = 12, Nome = "financeiro.editar", Descricao = "Editar Transações" },
                new Permissao { Id = 13, Nome = "financeiro.excluir", Descricao = "Excluir Transações" },
                
                new Permissao { Id = 14, Nome = "usuarios.listar", Descricao = "Listar Usuários" },
                new Permissao { Id = 15, Nome = "usuarios.criar", Descricao = "Criar Usuários" },
                new Permissao { Id = 16, Nome = "usuarios.editar", Descricao = "Editar Usuários" },
                new Permissao { Id = 17, Nome = "usuarios.excluir", Descricao = "Excluir Usuários" },
                
                new Permissao { Id = 18, Nome = "perfis.listar", Descricao = "Listar Perfis" },
                new Permissao { Id = 19, Nome = "perfis.criar", Descricao = "Criar Perfis" },
                new Permissao { Id = 20, Nome = "perfis.editar", Descricao = "Editar Perfis" },
                new Permissao { Id = 21, Nome = "perfis.excluir", Descricao = "Excluir Perfis" }
            );

            modelBuilder.Entity<RolePermissao>().HasData(
                new RolePermissao { Id = 1, RoleId = 1, PermissaoId = 1 },
                new RolePermissao { Id = 2, RoleId = 1, PermissaoId = 2 },
                new RolePermissao { Id = 3, RoleId = 1, PermissaoId = 3 },
                new RolePermissao { Id = 4, RoleId = 1, PermissaoId = 4 },
                new RolePermissao { Id = 5, RoleId = 1, PermissaoId = 5 },
                new RolePermissao { Id = 6, RoleId = 1, PermissaoId = 6 },
                new RolePermissao { Id = 7, RoleId = 1, PermissaoId = 7 },
                new RolePermissao { Id = 8, RoleId = 1, PermissaoId = 8 },
                new RolePermissao { Id = 9, RoleId = 1, PermissaoId = 9 },
                new RolePermissao { Id = 10, RoleId = 1, PermissaoId = 10 },
                new RolePermissao { Id = 11, RoleId = 1, PermissaoId = 11 },
                new RolePermissao { Id = 12, RoleId = 1, PermissaoId = 12 },
                new RolePermissao { Id = 13, RoleId = 1, PermissaoId = 13 },
                new RolePermissao { Id = 14, RoleId = 1, PermissaoId = 14 },
                new RolePermissao { Id = 15, RoleId = 1, PermissaoId = 15 },
                new RolePermissao { Id = 16, RoleId = 1, PermissaoId = 16 },
                new RolePermissao { Id = 17, RoleId = 1, PermissaoId = 17 },
                new RolePermissao { Id = 18, RoleId = 1, PermissaoId = 18 },
                new RolePermissao { Id = 19, RoleId = 1, PermissaoId = 19 },
                new RolePermissao { Id = 20, RoleId = 1, PermissaoId = 20 },
                new RolePermissao { Id = 21, RoleId = 1, PermissaoId = 21 },
                
                // Vendedor - Dashboard, Projetos e Vendas
                new RolePermissao { Id = 22, RoleId = 2, PermissaoId = 1 },
                new RolePermissao { Id = 23, RoleId = 2, PermissaoId = 2 },
                new RolePermissao { Id = 24, RoleId = 2, PermissaoId = 3 },
                new RolePermissao { Id = 25, RoleId = 2, PermissaoId = 4 },
                new RolePermissao { Id = 26, RoleId = 2, PermissaoId = 6 },
                new RolePermissao { Id = 27, RoleId = 2, PermissaoId = 7 },
                new RolePermissao { Id = 28, RoleId = 2, PermissaoId = 8 },
                
                // Financeiro - Dashboard e Financeiro
                new RolePermissao { Id = 29, RoleId = 3, PermissaoId = 1 },
                new RolePermissao { Id = 30, RoleId = 3, PermissaoId = 10 },
                new RolePermissao { Id = 31, RoleId = 3, PermissaoId = 11 },
                new RolePermissao { Id = 32, RoleId = 3, PermissaoId = 12 },
                new RolePermissao { Id = 33, RoleId = 3, PermissaoId = 13 }
            );
        }
    }
}
