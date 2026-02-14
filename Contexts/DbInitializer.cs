using Microsoft.EntityFrameworkCore;
using Nexo.Models;
using System.Security.Cryptography;
using System.Text;

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
                new() { Nome = "usuarios.visualizar", Descricao = "Visualizar usuários" },
                new() { Nome = "usuarios.criar", Descricao = "Criar usuários" },
                new() { Nome = "usuarios.editar", Descricao = "Editar usuários" },
                new() { Nome = "perfis.visualizar", Descricao = "Visualizar perfis" },
                new() { Nome = "perfis.criar", Descricao = "Criar perfis" }
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

            // 👤 4. Criar usuário Admin
            var adminUser = new Usuario
            {
                Nome = "Administrador",
                Email = "admin@nexo.com",
                Senha = HashSenha("Admin@123"),
                RoleId = adminRole.Id,
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Usuarios.AddAsync(adminUser);
            await context.SaveChangesAsync();
        }

        // 🔐 Hash simples (substitua por BCrypt em produção)
        private static string HashSenha(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(senha);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
