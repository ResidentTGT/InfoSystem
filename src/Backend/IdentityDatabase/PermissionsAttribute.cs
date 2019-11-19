using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Company.Common.Models.Identity;

namespace IdentityDatabase
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermissionsAttribute : ActionFilterAttribute
    {
        private IdentityContext _context { get; set; }

        public string PermissionName { get; set; }

        public Methods PermissionMethods { get; set; } = Methods.ReadWrite;

        public PermissionsAttribute()
        {
            _context = new IdentityContext(new DbContextOptions<IdentityContext>());
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            ValidRequest(actionContext);

            base.OnActionExecuting(actionContext);
        }

        private void ValidRequest(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            var appId = request.Headers["AppId"];
            var apiKey = request.Headers["ApiKey"];

            /* var app = await _context.ExternalApplications.Include(a => a.Permissions).FirstOrDefaultAsync(a => a.Id.ToString() == appId);

             if (app == null || app.ApiKey != apiKey)
                 context.Result = new BadRequestObjectResult("Неверный AppId или ApiKey приложения.") { StatusCode = StatusCodes.Status401Unauthorized };

             else if (app.Permissions == null || !app.Permissions.Any(p => p.Name == PermissionName && p.Methods.HasFlag(PermissionMethods)))
                 context.Result = new BadRequestObjectResult($"Недостаточно прав доступа. Необходимо разрешение {PermissionName}: {PermissionMethods}.") { StatusCode = StatusCodes.Status403Forbidden };*/
        }
    }
}
