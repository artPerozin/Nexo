using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Data;
using Nexo.Helpers;

namespace Nexo.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuthorizationHelper _authHelper;

        public DashboardController(AppDbContext context)
        {
            _context = context;
            _authHelper = new AuthorizationHelper(context);
        }

        public async Task<IActionResult> Index()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            
            if (usuarioId == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == usuarioId.Value);

            if (usuario == null || !usuario.Ativo)
            {
                HttpContext.Session.Clear();
                return RedirectToAction("Login", "Usuario");
            }

            // Redireciona para a dashboard específica baseada no role
            return usuario.Role.Nome switch
            {
                "Admin" => View("Admin"),
                "Gerente" => View("Gerente"),
                "Usuario" => View("Usuario"),
                _ => View("Usuario")
            };
        }

        [Permissao("dashboard.admin")]
        public async Task<IActionResult> Admin()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var usuariosAtivos = await _context.Usuarios.CountAsync(u => u.Ativo);
            var totalRoles = await _context.Roles.CountAsync();
            var totalPermissoes = await _context.Permissoes.CountAsync();

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.UsuariosAtivos = usuariosAtivos;
            ViewBag.TotalRoles = totalRoles;
            ViewBag.TotalPermissoes = totalPermissoes;

            return View();
        }

        [Permissao("dashboard.gerente")]
        public async Task<IActionResult> Gerente()
        {
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var usuariosAtivos = await _context.Usuarios.CountAsync(u => u.Ativo);

            ViewBag.TotalUsuarios = totalUsuarios;
            ViewBag.UsuariosAtivos = usuariosAtivos;

            return View();
        }

        [Permissao("dashboard.usuario")]
        public async Task<IActionResult> Usuario()
        {
            var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            return View(usuario);
        }
    }
}