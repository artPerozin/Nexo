using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Contexts;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class PerfisController(NexoContext context) : Controller
    {
        private readonly NexoContext _context = context;

        [Permissao("perfis.listar")]
        [Permissao("perfis.listar")]
        public async Task<IActionResult> Index()
        {
            var perfis = await _context.Roles
                .Include(r => r.RolePermissoes)
                .ThenInclude(rp => rp.Permissao)
                .OrderBy(r => r.Nome)
                .ToListAsync();

            var permissoes = await _context.Permissoes
                .OrderBy(p => p.Nome)
                .ToListAsync();

            ViewBag.Permissoes = permissoes;

            var permissoesAgrupadas = permissoes
                .GroupBy(p => p.Nome.Split('.')[0])
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            ViewBag.PermissoesAgrupadas = permissoesAgrupadas;

            return View(perfis);
        }

        [Permissao("perfis.criar")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] PerfilCriarModel model)
        {
            try
            {
                if (await _context.Roles.AnyAsync(r => r.Nome == model.Nome))
                {
                    return Json(new { success = false, message = "Já existe um perfil com este nome!" });
                }

                var role = new Role
                {
                    Nome = model.Nome,
                    Descricao = model.Descricao
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                // Adicionar permissões
                if (model.PermissoesIds != null && model.PermissoesIds.Any())
                {
                    foreach (var permissaoId in model.PermissoesIds)
                    {
                        _context.RolePermissoes.Add(new RolePermissao
                        {
                            RoleId = role.Id,
                            PermissaoId = permissaoId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Perfil criado com sucesso!", perfilId = role.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao criar perfil: {ex.Message}" });
            }
        }

        [Permissao("perfis.editar")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] PerfilEditarModel model)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.RolePermissoes)
                    .FirstOrDefaultAsync(r => r.Id == model.Id);
                
                if (role == null)
                    return Json(new { success = false, message = "Perfil não encontrado!" });

                if (await _context.Roles.AnyAsync(r => r.Nome == model.Nome && r.Id != model.Id))
                {
                    return Json(new { success = false, message = "Já existe um perfil com este nome!" });
                }

                role.Nome = model.Nome;
                role.Descricao = model.Descricao;

                // Remover permissões antigas
                _context.RolePermissoes.RemoveRange(role.RolePermissoes);

                // Adicionar novas permissões
                if (model.PermissoesIds != null && model.PermissoesIds.Any())
                {
                    foreach (var permissaoId in model.PermissoesIds)
                    {
                        _context.RolePermissoes.Add(new RolePermissao
                        {
                            RoleId = role.Id,
                            PermissaoId = permissaoId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Perfil atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao atualizar perfil: {ex.Message}" });
            }
        }

        [Permissao("perfis.excluir")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var role = await _context.Roles
                    .Include(r => r.Usuarios)
                    .FirstOrDefaultAsync(r => r.Id == id);
                
                if (role == null)
                    return Json(new { success = false, message = "Perfil não encontrado!" });

                if (role.Usuarios.Any())
                {
                    return Json(new { success = false, message = "Não é possível excluir um perfil que possui usuários vinculados!" });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Perfil excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao excluir perfil: {ex.Message}" });
            }
        }

        [Permissao("perfis.listar")]
        public async Task<IActionResult> ObterPerfil(int id)
        {
            var role = await _context.Roles
                .Include(r => r.RolePermissoes)
                .ThenInclude(rp => rp.Permissao)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
                return Json(new { success = false, message = "Perfil não encontrado!" });

            return Json(new { 
                success = true, 
                perfil = new {
                    id = role.Id,
                    nome = role.Nome,
                    descricao = role.Descricao,
                    permissoesIds = role.RolePermissoes.Select(rp => rp.PermissaoId).ToList()
                }
            });
        }

        [Permissao("perfis.listar")]
        public async Task<IActionResult> ObterPermissoes()
        {
            var permissoes = await _context.Permissoes
                .OrderBy(p => p.Nome)
                .Select(p => new {
                    id = p.Id,
                    nome = p.Nome,
                    descricao = p.Descricao
                })
                .ToListAsync();

            return Json(new { success = true, permissoes });
        }
    }

    public class PerfilCriarModel
    {
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
        public List<int>? PermissoesIds { get; set; }
    }

    public class PerfilEditarModel
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string? Descricao { get; set; }
        public List<int>? PermissoesIds { get; set; }
    }
}