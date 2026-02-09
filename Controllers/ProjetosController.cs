using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Data;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class ProjetosController : Controller
    {
        private readonly AppDbContext _context;

        public ProjetosController(AppDbContext context)
        {
            _context = context;
        }

        [Permissao("projetos.listar")]
        public async Task<IActionResult> Index()
        {
            var projetos = await _context.Projetos
                .Include(p => p.Responsavel)
                .OrderByDescending(p => p.DataCriacao)
                .ToListAsync();

            return View(projetos);
        }

        [Permissao("projetos.criar")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Projeto projeto)
        {
            try
            {
                projeto.DataCriacao = DateTime.UtcNow;
                _context.Projetos.Add(projeto);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Projeto criado com sucesso!", projetoId = projeto.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao criar projeto: {ex.Message}" });
            }
        }

        [Permissao("projetos.editar")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] Projeto projeto)
        {
            try
            {
                var projetoExistente = await _context.Projetos.FindAsync(projeto.Id);
                
                if (projetoExistente == null)
                    return Json(new { success = false, message = "Projeto não encontrado!" });

                projetoExistente.Nome = projeto.Nome;
                projetoExistente.Descricao = projeto.Descricao;
                projetoExistente.Status = projeto.Status;
                projetoExistente.Progresso = projeto.Progresso;
                projetoExistente.DataFim = projeto.DataFim;
                projetoExistente.Valor = projeto.Valor;
                projetoExistente.ResponsavelId = projeto.ResponsavelId;
                projetoExistente.DataAtualizacao = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Projeto atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao atualizar projeto: {ex.Message}" });
            }
        }

        [Permissao("projetos.excluir")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var projeto = await _context.Projetos.FindAsync(id);
                
                if (projeto == null)
                    return Json(new { success = false, message = "Projeto não encontrado!" });

                _context.Projetos.Remove(projeto);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Projeto excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao excluir projeto: {ex.Message}" });
            }
        }

        [Permissao("projetos.listar")]
        public async Task<IActionResult> Detalhes(int id)
        {
            var projeto = await _context.Projetos
                .Include(p => p.Responsavel)
                .Include(p => p.Tarefas)
                .ThenInclude(t => t.Responsavel)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return NotFound();

            return View(projeto);
        }

        [Permissao("projetos.listar")]
        public async Task<IActionResult> ObterProjeto(int id)
        {
            var projeto = await _context.Projetos
                .Include(p => p.Responsavel)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (projeto == null)
                return Json(new { success = false, message = "Projeto não encontrado!" });

            return Json(new { success = true, projeto });
        }
    }
}
