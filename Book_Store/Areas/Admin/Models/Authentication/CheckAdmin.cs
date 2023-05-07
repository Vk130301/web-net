using Book_Store.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using NToastNotify;

namespace Book_Store.Areas.Admin.Models.Authentication
{
    public class CheckAdmin : ActionFilterAttribute
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
                        {"Action", "AdminLogin" },
                    });

                return;
            }

            var roleString = context.HttpContext.Session.GetString("Description");

            if (string.IsNullOrEmpty(roleString))
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Area", "Admin" },
                        {"Controller", "Account" },
                        {"Action", "AdminLogin" },
                    });

                return;
            }

            var role = new Role() { Description = roleString };

            if (role.Description != "Admin")
            {
                context.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        {"Area", "Admin" },
                        {"Controller", "Account" },
                        {"Action", "AdminLogin" },
                    });

                return;
            }
        }
    }
}
