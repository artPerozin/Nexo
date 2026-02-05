using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nexo.Helpers;
using Nexo.Data;

namespace Nexo.Attributes
{
    public class PermissaoAttribute : TypeFilterAttribute
    {
        public PermissaoAttribute(string permissao) : base(typeof(PermissaoFilter))
        {
            Arguments = new object[] { permissao };
        }
    }

    public class PermissaoFilter : IAsyncActionFilter
    {
        private readonly string _permissao;
        private readonly AppDbContext _context;

        public PermissaoFilter(string permissao, AppDbContext context)
        {
            _permissao = permissao;
            _context = context;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Usuario" },
                        { "action", "Login" }
                    });
                return;
            }

            var authHelper = new AuthorizationHelper(_context);
            var temPermissao = await authHelper.UsuarioTemPermissao(usuarioId.Value, _permissao);

            if (!temPermissao)
            {
                context.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AcessoNegado.cshtml"
                };
                return;
            }

            await next();
        }
    }
}