using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Company.Common.Models.Identity;
using WebApi.Dto.Users;

namespace WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleAttribute : ActionFilterAttribute
    {
        private readonly string[] _roles;
        private readonly Module _module;

        public RoleAttribute(Module module, string[] roles)
        {
            _module = module;
            _roles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var user = actionContext.HttpContext.Items["User"] as User;
            if (user == null)
            {
                actionContext.Result = new UnauthorizedResult();
                return;
            }

            var userRoles = user.UserRoles.Where(r => r.Role.Module == _module).Select(r => r.Role.Name);
            if (!_roles.Intersect(userRoles).Any())
            {
                actionContext.Result = new ForbidResult();
            }
        }
    }
}

