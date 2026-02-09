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

        [Permissao("dashboard.visualizar")]
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

            // Métricas Gerais
            var projetosAtivos = await _context.Projetos.CountAsync(p => p.Status != "Concluído");
            var dealsAbertos = await _context.Deals.CountAsync(d => d.Estagio != "Fechado");
            
            var transacoes = await _context.Transacoes.ToListAsync();
            var receitaTotal = transacoes.Where(t => t.Tipo == "Receita").Sum(t => t.Valor);
            var despesaTotal = transacoes.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor);
            var saldo = receitaTotal - despesaTotal;

            // Pipeline
            var pipelineValor = await _context.Deals
                .Where(d => d.Estagio != "Fechado")
                .SumAsync(d => d.Valor);

            // Últimas transações
            var ultimasTransacoes = await _context.Transacoes
                .Include(t => t.CriadoPor)
                .OrderByDescending(t => t.Data)
                .Take(5)
                .ToListAsync();

            // Projetos recentes
            var projetosRecentes = await _context.Projetos
                .Include(p => p.Responsavel)
                .OrderByDescending(p => p.DataCriacao)
                .Take(5)
                .ToListAsync();

            ViewBag.ProjetosAtivos = projetosAtivos;
            ViewBag.DealsAbertos = dealsAbertos;
            ViewBag.Pipeline = pipelineValor;
            ViewBag.Saldo = saldo;
            ViewBag.ReceitaTotal = receitaTotal;
            ViewBag.DespesaTotal = despesaTotal;
            ViewBag.UltimasTransacoes = ultimasTransacoes;
            ViewBag.ProjetosRecentes = projetosRecentes;
            ViewBag.Usuario = usuario;

            return View();
        }

        [Permissao("dashboard.visualizar")]
        public async Task<IActionResult> ObterMetricas()
        {
            var metricas = new
            {
                projetos = new
                {
                    total = await _context.Projetos.CountAsync(),
                    ativos = await _context.Projetos.CountAsync(p => p.Status == "Em andamento"),
                    planejamento = await _context.Projetos.CountAsync(p => p.Status == "Planejamento"),
                    concluidos = await _context.Projetos.CountAsync(p => p.Status == "Concluído")
                },
                vendas = new
                {
                    total = await _context.Deals.CountAsync(),
                    valorTotal = await _context.Deals.SumAsync(d => d.Valor),
                    fechados = await _context.Deals.CountAsync(d => d.Estagio == "Fechado"),
                    pipeline = await _context.Deals.Where(d => d.Estagio != "Fechado").SumAsync(d => d.Valor),
                    porEstagio = await _context.Deals
                        .GroupBy(d => d.Estagio)
                        .Select(g => new { estagio = g.Key, quantidade = g.Count(), valor = g.Sum(d => d.Valor) })
                        .ToListAsync()
                },
                financeiro = new
                {
                    receita = await _context.Transacoes.Where(t => t.Tipo == "Receita").SumAsync(t => t.Valor),
                    despesa = await _context.Transacoes.Where(t => t.Tipo == "Despesa").SumAsync(t => t.Valor),
                    pendente = await _context.Transacoes.Where(t => t.Status == "Pendente").SumAsync(t => t.Valor),
                    vencido = await _context.Transacoes.Where(t => t.Status == "Vencido").SumAsync(t => t.Valor)
                }
            };

            return Json(metricas);
        }

        [Permissao("dashboard.visualizar")]
        public async Task<IActionResult> ObterGraficoFinanceiro(int meses = 6)
        {
            var dataInicio = DateTime.Now.AddMonths(-meses);
            
            var transacoes = await _context.Transacoes
                .Where(t => t.Data >= dataInicio)
                .ToListAsync();

            var dadosPorMes = transacoes
                .GroupBy(t => new { t.Data.Year, t.Data.Month })
                .Select(g => new
                {
                    mes = $"{g.Key.Year}-{g.Key.Month:D2}",
                    receita = g.Where(t => t.Tipo == "Receita").Sum(t => t.Valor),
                    despesa = g.Where(t => t.Tipo == "Despesa").Sum(t => t.Valor)
                })
                .OrderBy(x => x.mes)
                .ToList();

            return Json(dadosPorMes);
        }
    }
}
