using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Book_Store.Areas.Admin.Models.Authentication
{
    public class Authentication: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("AccountId") == null)
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Area", "Admin" },
                        {"Controller", "Account" },
                        {"Action", "AdminLogin" }
                    });
            }
        }
    }
}
