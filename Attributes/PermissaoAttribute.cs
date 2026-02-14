using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Nexo.Helpers;

namespace Nexo.Attributes
{
    public class PermissaoAttribute : TypeFilterAttribute
    {
        public PermissaoAttribute(string permissao)
            : base(typeof(PermissaoFilter))
        {
            Arguments = new object[] { permissao };
        }
    }

    public class PermissaoFilter : IAsyncActionFilter
    {
        private readonly string _permissao;
        private readonly AuthorizationHelper _authHelper;

        public PermissaoFilter(string permissao, AuthorizationHelper authHelper)
        {
            _permissao = permissao;
            _authHelper = authHelper;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var usuarioId = context.HttpContext.Session.GetInt32("UsuarioId");

            // 🔐 Não logado
            if (usuarioId == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary {
                        { "controller", "Usuario" },
                        { "action", "Login" }
                    });

                return;
            }

            // 🔐 Verifica permissão
            var temPermissao = await _authHelper
                .UsuarioTemPermissao(usuarioId.Value, _permissao);

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
