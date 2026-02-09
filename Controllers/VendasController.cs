using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Data;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class VendasController : Controller
    {
        private readonly AppDbContext _context;

        public VendasController(AppDbContext context)
        {
            _context = context;
        }

        [Permissao("vendas.listar")]
        public async Task<IActionResult> Index()
        {
            var deals = await _context.Deals
                .Include(d => d.Responsavel)
                .OrderByDescending(d => d.DataCriacao)
                .ToListAsync();

            ViewBag.Estagios = new List<string> { "Prospecção", "Qualificação", "Proposta", "Negociação", "Fechado" };

            return View(deals);
        }

        [Permissao("vendas.criar")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Deal deal)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                
                deal.ResponsavelId = usuarioId;
                deal.DataCriacao = DateTime.UtcNow;
                
                _context.Deals.Add(deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal criado com sucesso!", dealId = deal.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao criar deal: {ex.Message}" });
            }
        }

        [Permissao("vendas.editar")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] Deal deal)
        {
            try
            {
                var dealExistente = await _context.Deals.FindAsync(deal.Id);
                
                if (dealExistente == null)
                    return Json(new { success = false, message = "Deal não encontrado!" });

                dealExistente.Nome = deal.Nome;
                dealExistente.Descricao = deal.Descricao;
                dealExistente.Valor = deal.Valor;
                dealExistente.Estagio = deal.Estagio;
                dealExistente.Cliente = deal.Cliente;
                dealExistente.EmailCliente = deal.EmailCliente;
                dealExistente.TelefoneCliente = deal.TelefoneCliente;
                dealExistente.DataFechamentoEstimada = deal.DataFechamentoEstimada;
                dealExistente.Probabilidade = deal.Probabilidade;
                dealExistente.DataAtualizacao = DateTime.UtcNow;

                if (deal.Estagio == "Fechado" && dealExistente.DataFechamento == null)
                {
                    dealExistente.DataFechamento = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao atualizar deal: {ex.Message}" });
            }
        }

        [Permissao("vendas.editar")]
        [HttpPost]
        public async Task<IActionResult> MoverEstagio([FromBody] MoverEstagioRequest request)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(request.DealId);
                
                if (deal == null)
                    return Json(new { success = false, message = "Deal não encontrado!" });

                deal.Estagio = request.NovoEstagio;
                deal.DataAtualizacao = DateTime.UtcNow;

                if (request.NovoEstagio == "Fechado" && deal.DataFechamento == null)
                {
                    deal.DataFechamento = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal movido com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao mover deal: {ex.Message}" });
            }
        }

        [Permissao("vendas.excluir")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(id);
                
                if (deal == null)
                    return Json(new { success = false, message = "Deal não encontrado!" });

                _context.Deals.Remove(deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal excluído com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao excluir deal: {ex.Message}" });
            }
        }

        [Permissao("vendas.listar")]
        public async Task<IActionResult> ObterDeal(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Responsavel)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return Json(new { success = false, message = "Deal não encontrado!" });

            return Json(new { success = true, deal });
        }

        [Permissao("vendas.listar")]
        public async Task<IActionResult> ObterEstatisticas()
        {
            var deals = await _context.Deals.ToListAsync();

            var estatisticas = new
            {
                total = deals.Count,
                valorTotal = deals.Sum(d => d.Valor),
                porEstagio = deals.GroupBy(d => d.Estagio)
                    .Select(g => new { estagio = g.Key, quantidade = g.Count(), valor = g.Sum(d => d.Valor) })
                    .ToList(),
                taxaConversao = deals.Count > 0 
                    ? (decimal)deals.Count(d => d.Estagio == "Fechado") / deals.Count * 100 
                    : 0
            };

            return Json(estatisticas);
        }
    }

    public class MoverEstagioRequest
    {
        public int DealId { get; set; }
        public string NovoEstagio { get; set; } = string.Empty;
    }
}
