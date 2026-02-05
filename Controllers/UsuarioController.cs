using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Data;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly AppDbContext _context;

        public UsuarioController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Usuario/Cadastro
        public IActionResult Cadastro()
        {
            return View();
        }

        // POST: Usuario/Cadastro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cadastro(Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Verifica se o email já existe
                    if (await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email))
                    {
                        return Json(new { success = false, message = "Este email já está cadastrado!" });
                    }

                    // Hash da senha
                    usuario.Senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
                    
                    // Define o role padrão como "Usuario" (ID 3)
                    usuario.RoleId = 3;
                    usuario.Ativo = true;
                    usuario.DataCriacao = DateTime.UtcNow;

                    _context.Usuarios.Add(usuario);
                    await _context.SaveChangesAsync();

                    return Json(new { success = true, message = "Cadastro realizado com sucesso!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Erro ao cadastrar: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join("<br>", errors) });
        }

        // GET: Usuario/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Usuario/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var usuario = await _context.Usuarios
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (usuario != null && usuario.Ativo && BCrypt.Net.BCrypt.Verify(model.Senha, usuario.Senha))
                    {
                        HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
                        HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                        HttpContext.Session.SetString("UsuarioEmail", usuario.Email);
                        HttpContext.Session.SetInt32("UsuarioRoleId", usuario.RoleId);

                        return Json(new { success = true, message = "Login realizado com sucesso!" });
                    }

                    if (usuario != null && !usuario.Ativo)
                    {
                        return Json(new { success = false, message = "Usuário inativo. Entre em contato com o administrador." });
                    }

                    return Json(new { success = false, message = "Email ou senha inválidos!" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Erro ao fazer login: " + ex.Message });
                }
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return Json(new { success = false, message = string.Join("<br>", errors) });
        }

        // GET: Usuario/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}