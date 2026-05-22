using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DocLocker.Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        private readonly HashSet<string> _allowedRoles;

        public SessionAuthorizeAttribute(params string[] allowedRoles)
        {
            _allowedRoles = allowedRoles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = context.HttpContext.Session.GetString("Token");
            var role = context.HttpContext.Session.GetString("Role");

            if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(role))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            if (_allowedRoles.Count > 0 && !_allowedRoles.Contains(role))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }
    }
}
