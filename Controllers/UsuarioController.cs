using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Contexts;
using Nexo.Models;
using Nexo.Models.Dtos;
using Nexo.Models.DTOs;

namespace Nexo.Controllers
{
    public class UsuarioController(NexoContext context) : Controller
    {
        private readonly NexoContext _context = context;

        #region Helpers

        private IActionResult Success(string message, object? data = null)
            => Json(new { success = true, message, data });

        private IActionResult Error(string message)
            => Json(new { success = false, message });

        private int? UsuarioLogadoId()
            => HttpContext.Session.GetInt32("UsuarioId");

        private void CriarSessao(Usuario usuario)
        {
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
            HttpContext.Session.SetInt32("RoleId", usuario.RoleId);
        }

        private void EncerrarSessao()
        {
            HttpContext.Session.Clear();
        }

        #endregion

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult Login()
        {
            if (UsuarioLogadoId() != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Dados inválidos." });

            var usuario = await _context.Usuarios
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha))
                return Json(new { success = false, message = "E-mail ou senha inválidos." });

            if (!usuario.Ativo)
                return Json(new { success = false, message = "Usuário desativado." });

            // 🔥 AQUI ESTÁ O QUE FALTA
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
            HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
            HttpContext.Session.SetString("UsuarioRole", usuario.Role?.Nome ?? "");

            return Json(new { success = true, message = "Login realizado com sucesso." });
        }


        public IActionResult Logout()
        {
            EncerrarSessao();
            return RedirectToAction(nameof(Login));
        }

        // =========================
        // LISTAGEM
        // =========================

        [Permissao("usuarios.listar")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .AsNoTracking()
                .Include(u => u.Role)
                .OrderBy(u => u.Nome)
                .ToListAsync();

            ViewBag.Roles = await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.Nome)
                .ToListAsync();

            return View(usuarios);
        }

        // =========================
        // CRIAR
        // =========================

        [Permissao("usuarios.criar")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] UsuarioCriarDto model)
        {
            if (!ModelState.IsValid)
                return Error("Dados inválidos.");

            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
                return Error("E-mail já cadastrado.");

            if (!await _context.Roles.AnyAsync(r => r.Id == model.RoleId))
                return Error("Perfil inválido.");

            var usuario = new Usuario
            {
                Nome = model.Nome,
                Email = model.Email.Trim().ToLower(),
                Senha = BCrypt.Net.BCrypt.HashPassword(model.Senha),
                RoleId = model.RoleId,
                Ativo = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Success("Usuário criado com sucesso.", new { usuario.Id });
        }

        // =========================
        // EDITAR
        // =========================

        [Permissao("usuarios.editar")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] UsuarioEditarDto model)
        {
            if (!ModelState.IsValid)
                return Error("Dados inválidos.");

            var usuario = await _context.Usuarios.FindAsync(model.Id);

            if (usuario == null)
                return Error("Usuário não encontrado.");

            if (await _context.Usuarios
                .AnyAsync(u => u.Email == model.Email && u.Id != model.Id))
                return Error("E-mail já está em uso.");

            if (!await _context.Roles.AnyAsync(r => r.Id == model.RoleId))
                return Error("Perfil inválido.");

            usuario.Nome = model.Nome;
            usuario.Email = model.Email.Trim().ToLower();
            usuario.RoleId = model.RoleId;
            usuario.Ativo = model.Ativo;
            usuario.DataAtualizacao = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(model.Senha))
                usuario.Senha = BCrypt.Net.BCrypt.HashPassword(model.Senha);

            await _context.SaveChangesAsync();

            return Success("Usuário atualizado com sucesso.");
        }

        // =========================
        // ALTERAR STATUS
        // =========================

        [Permissao("usuarios.editar")]
        [HttpPost]
        public async Task<IActionResult> AlterarStatus(int id)
        {
            var usuarioLogadoId = UsuarioLogadoId();

            if (usuarioLogadoId == null)
                return Error("Sessão inválida.");

            if (id == usuarioLogadoId)
                return Error("Você não pode alterar seu próprio status.");

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return Error("Usuário não encontrado.");

            usuario.Ativo = !usuario.Ativo;
            usuario.DataAtualizacao = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Success($"Usuário {(usuario.Ativo ? "ativado" : "desativado")} com sucesso.");
        }

        [HttpGet]
        public async Task<IActionResult> ObterUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            return Json(usuario);
        }
    }
}
