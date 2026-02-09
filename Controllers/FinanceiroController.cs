using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexo.Attributes;
using Nexo.Data;
using Nexo.Models;

namespace Nexo.Controllers
{
    public class FinanceiroController : Controller
    {
        private readonly AppDbContext _context;

        public FinanceiroController(AppDbContext context)
        {
            _context = context;
        }

        [Permissao("financeiro.visualizar")]
        public async Task<IActionResult> Index()
        {
            var transacoes = await _context.Transacoes
                .Include(t => t.CriadoPor)
                .Include(t => t.Projeto)
                .Include(t => t.Deal)
                .OrderByDescending(t => t.Data)
                .ToListAsync();

            var receitaTotal = transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor);
            var despesaTotal = transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor);
            var pendente = transacoes.Where(t => t.Status == "Pendente").Sum(t => t.Valor);
            var vencido = transacoes.Where(t => t.Status == "Vencido").Sum(t => t.Valor);

            ViewBag.ReceitaTotal = receitaTotal;
            ViewBag.DespesaTotal = despesaTotal;
            ViewBag.Pendente = pendente;
            ViewBag.Vencido = vencido;
            ViewBag.Saldo = receitaTotal - despesaTotal;

            return View(transacoes);
        }

        [Permissao("financeiro.criar")]
        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] Transacao transacao)
        {
            try
            {
                var usuarioId = HttpContext.Session.GetInt32("UsuarioId");
                
                if (usuarioId == null)
                    return Json(new { success = false, message = "Usuário não autenticado!" });

                transacao.CriadoPorId = usuarioId.Value;
                transacao.DataCriacao = DateTime.UtcNow;
                
                _context.Transacoes.Add(transacao);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Transação criada com sucesso!", transacaoId = transacao.Id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao criar transação: {ex.Message}" });
            }
        }

        [Permissao("financeiro.editar")]
        [HttpPost]
        public async Task<IActionResult> Editar([FromBody] Transacao transacao)
        {
            try
            {
                var transacaoExistente = await _context.Transacoes.FindAsync(transacao.Id);
                
                if (transacaoExistente == null)
                    return Json(new { success = false, message = "Transação não encontrada!" });

                transacaoExistente.Descricao = transacao.Descricao;
                transacaoExistente.Tipo = transacao.Tipo;
                transacaoExistente.Valor = transacao.Valor;
                transacaoExistente.Categoria = transacao.Categoria;
                transacaoExistente.Data = transacao.Data;
                transacaoExistente.Status = transacao.Status;
                transacaoExistente.DataVencimento = transacao.DataVencimento;
                transacaoExistente.DataPagamento = transacao.DataPagamento;
                transacaoExistente.FormaPagamento = transacao.FormaPagamento;
                transacaoExistente.Observacoes = transacao.Observacoes;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Transação atualizada com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao atualizar transação: {ex.Message}" });
            }
        }

        [Permissao("financeiro.editar")]
        [HttpPost]
        public async Task<IActionResult> MarcarComoPago(int id)
        {
            try
            {
                var transacao = await _context.Transacoes.FindAsync(id);
                
                if (transacao == null)
                    return Json(new { success = false, message = "Transação não encontrada!" });

                transacao.Status = "Pago";
                transacao.DataPagamento = DateTime.Now;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Transação marcada como paga!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao marcar transação: {ex.Message}" });
            }
        }

        [Permissao("financeiro.excluir")]
        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                var transacao = await _context.Transacoes.FindAsync(id);
                
                if (transacao == null)
                    return Json(new { success = false, message = "Transação não encontrada!" });

                _context.Transacoes.Remove(transacao);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Transação excluída com sucesso!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro ao excluir transação: {ex.Message}" });
            }
        }

        [Permissao("financeiro.visualizar")]
        public async Task<IActionResult> ObterTransacao(int id)
        {
            var transacao = await _context.Transacoes
                .Include(t => t.CriadoPor)
                .Include(t => t.Projeto)
                .Include(t => t.Deal)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (transacao == null)
                return Json(new { success = false, message = "Transação não encontrada!" });

            return Json(new { success = true, transacao });
        }

        [Permissao("financeiro.visualizar")]
        public async Task<IActionResult> ObterEstatisticas(DateTime? dataInicio, DateTime? dataFim)
        {
            var query = _context.Transacoes.AsQueryable();

            if (dataInicio.HasValue)
                query = query.Where(t => t.Data >= dataInicio.Value);

            if (dataFim.HasValue)
                query = query.Where(t => t.Data <= dataFim.Value);

            var transacoes = await query.ToListAsync();

            var estatisticas = new
            {
                receitaTotal = transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor),
                despesaTotal = transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor),
                saldo = transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor) - 
                        transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor),
                pendente = transacoes.Where(t => t.Status == "Pendente").Sum(t => t.Valor),
                pago = transacoes.Where(t => t.Status == "Pago").Sum(t => t.Valor),
                vencido = transacoes.Where(t => t.Status == "Vencido").Sum(t => t.Valor),
                porCategoria = transacoes.GroupBy(t => t.Categoria)
                    .Select(g => new { categoria = g.Key, valor = g.Sum(t => t.Valor) })
                    .OrderByDescending(x => x.valor)
                    .ToList(),
                porMes = transacoes.GroupBy(t => new { t.Data.Year, t.Data.Month })
                    .Select(g => new 
                    { 
                        mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                        receita = g.Where(t => t.Tipo == "Receita").Sum(t => t.Valor),
                        despesa = g.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor)
                    })
                    .OrderBy(x => x.mes)
                    .ToList()
            };

            return Json(estatisticas);
        }
    }
}
