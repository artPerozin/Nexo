using Microsoft.EntityFrameworkCore;
using Nexo.Models;

namespace Nexo.Contexts
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(NexoContext context)
        {
            await context.Database.MigrateAsync();

            // Se já existir admin, não faz nada
            if (await context.Usuarios.AnyAsync())
                return;

            // 🔐 1. Criar Permissões
            var permissoes = new List<Permissao>
            {
                new() { Nome = "dashboard.visualizar", Descricao = "Visualizar dashboard" },
                new() { Nome = "usuarios.listar", Descricao = "Listar usuários" },
                new() { Nome = "usuarios.criar", Descricao = "Criar usuários" },
                new() { Nome = "usuarios.editar", Descricao = "Editar usuários" },
                new() { Nome = "perfis.listar", Descricao = "Listar perfis" },
                new() { Nome = "perfis.criar", Descricao = "Criar perfis" },
                new() { Nome = "perfis.editar", Descricao = "Editar perfis" },
                new() { Nome = "perfis.excluir", Descricao = "Excluir perfis" }
            };

            await context.Permissoes.AddRangeAsync(permissoes);
            await context.SaveChangesAsync();

            // 👑 2. Criar Role Admin
            var adminRole = new Role
            {
                Nome = "Admin",
                Descricao = "Administrador do sistema"
            };

            await context.Roles.AddAsync(adminRole);
            await context.SaveChangesAsync();

            // 🔗 3. Associar todas permissões ao Admin
            var rolePermissoes = permissoes.Select(p => new RolePermissao
            {
                RoleId = adminRole.Id,
                PermissaoId = p.Id
            }).ToList();

            await context.RolePermissoes.AddRangeAsync(rolePermissoes);
            await context.SaveChangesAsync();

            // 👤 4. Criar usuário Admin (usando BCrypt)
            var adminUser = new Usuario
            {
                Nome = "Administrador",
                Email = "admin@nexo.com",
                Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                RoleId = adminRole.Id,
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Usuarios.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }
    }
}