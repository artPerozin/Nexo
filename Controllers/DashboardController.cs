using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Contexts;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class DashboardController : Controller
    {
        private readonly NexoContext _context;

        public DashboardController(NexoContext context)
        {
            _context = context;
        }

        [Permissao("dashboard.visualizar")]
        public async Task<IActionResult> Index()
        {
            var usuario = await ObterUsuarioLogado();

            if (usuario == null)
                return RedirectToAction("Login", "Usuario");

            return View(usuario);
        }

        [HttpGet]
        [Permissao("dashboard.visualizar")]
        public async Task<IActionResult> ObterMetricas()
        {
            var usuario = await ObterUsuarioLogado();

            if (usuario == null)
                return Unauthorized();

            var metricas = new
            {
                TotalUsuarios = await _context.Usuarios.CountAsync(),
                UsuariosAtivos = await _context.Usuarios.CountAsync(u => u.Ativo),
                TotalRoles = await _context.Roles.CountAsync(),
                TotalPermissoes = await _context.Permissoes.CountAsync()
            };

            return Json(metricas);
        }

        // 🔥 Método centralizado para evitar repetição
        private async Task<Usuario?> ObterUsuarioLogado()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return null;

            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                    .ThenInclude(r => r.RolePermissoes)
                        .ThenInclude(rp => rp.Permissao)
                .FirstOrDefaultAsync(u => u.Id == usuarioId.Value);

            if (usuario == null || !usuario.Ativo)
            {
                HttpContext.Session.Clear();
                return null;
            }

            return usuario;
        }
    }
}
