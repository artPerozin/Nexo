using Microsoft.EntityFrameworkCore;
using Nexo.Data;

namespace Nexo.Helpers
{
    public class AuthorizationHelper(AppDbContext context)
    {
        private readonly AppDbContext _context = context;

        public async Task<bool> UsuarioTemPermissao(int usuarioId, string nomePermissao)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissoes)
                .ThenInclude(rp => rp.Permissao)
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

            if (usuario == null) return false;

            return usuario.Role?.RolePermissoes
                .Any(rp => rp.Permissao.Nome == nomePermissao) ?? false;
        }

        public async Task<List<string>> ObterPermissoesUsuario(int usuarioId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .ThenInclude(r => r.RolePermissoes)
                .ThenInclude(rp => rp.Permissao)
                .FirstOrDefaultAsync(u => u.Id == usuarioId && u.Ativo);

            if (usuario == null) return new List<string>();

            return usuario.Role?.RolePermissoes
                .Select(rp => rp.Permissao.Nome)
                .ToList() ?? new List<string>();
        }
    }
}