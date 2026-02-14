using Microsoft.EntityFrameworkCore;
using Nexo.Contexts;

namespace Nexo.Helpers
{
    public class AuthorizationHelper
    {
        private readonly NexoContext _context;

        public AuthorizationHelper(NexoContext context)
        {
            _context = context;
        }

        public async Task<bool> UsuarioTemPermissao(int usuarioId, string nomePermissao)
        {
            return await _context.RolePermissoes
                .AsNoTracking()
                .Where(rp =>
                    rp.Role.Usuarios.Any(u => u.Id == usuarioId && u.Ativo) &&
                    rp.Permissao.Nome == nomePermissao)
                .AnyAsync();
        }

        public async Task<List<string>> ObterPermissoesUsuario(int usuarioId)
        {
            return await _context.RolePermissoes
                .AsNoTracking()
                .Where(rp =>
                    rp.Role.Usuarios.Any(u => u.Id == usuarioId && u.Ativo))
                .Select(rp => rp.Permissao.Nome)
                .ToListAsync();
        }
    }
}
