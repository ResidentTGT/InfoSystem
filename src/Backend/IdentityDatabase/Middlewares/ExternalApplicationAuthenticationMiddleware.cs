using IdentityDatabase.Dto;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Company.Common.Models.Identity;

namespace IdentityDatabase.Middlewares
{
    public class ExternalApplicationAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private IdentityContext _context;

        public ExternalApplicationAuthenticationMiddleware(RequestDelegate next, IdentityContext context)
        {
            _next = next;
            _context = context;
        }

        public async Task InvokeAsync(HttpContext httpContext, IdentityContext context, IPermissionDbContext permissions)
        {
            var appId = httpContext.Request.Headers["AppId"];
            var apiKey = httpContext.Request.Headers["ApiKey"];

            bool isId = int.TryParse(appId, out int id);

            if (isId && apiKey.ToString() != null)
            {
                httpContext.Items.Add("ExternalApplication", GetExternalApplicationWithPermissionsDto(id, apiKey, context, permissions));
            }
            else
            {
                // TODO Error
            }


            await _next(httpContext);
        }

        private ExternalApplicationDto GetExternalApplicationWithPermissionsDto(int id, string apiKey, IdentityContext context, IPermissionDbContext permissions)
        {
            var app = new
            {
                ExternalApplication = context.ExternalApplications.FirstOrDefault(a => a.Id == id),
                Roles = context.Roles.Include(r => r.UserRoles).Where(r => r.ExternalApplicationRoles.Select(ear => ear.ExternalApplicationId).Contains(id)).ToList()
            };

            if (app.ExternalApplication == null || app.ExternalApplication.ApiKey != apiKey)
            {
                return null;
            }

            var externalApplicationDto = new ExternalApplicationDto()
            {
                Id = app.ExternalApplication.Id,
                Name = app.ExternalApplication.Name,
                ApiKey = app.ExternalApplication.ApiKey
            };

            foreach (var module in app.Roles.GroupBy(r => r.Module).Select(g => g.Select(r => r.Module).First()).ToList())
            {
                var sectionPermissions = permissions.SectionPermissions
                    .Where(sp => app.Roles.Where(r => r.Module == module).Select(r => r.Id).Contains(sp.RoleId)).GroupBy(sp => sp.Name).Select(g => g.FirstOrDefault()).ToList();

                var resourcePermissionsList = permissions.ResourcePermissions
                    .Where(rp => app.Roles.Where(r => r.Module == module).Select(r => r.Id).Contains(rp.RoleId))
                    .GroupBy(rp => rp.Name)
                    .ToList()
                    .Select(g => new ResourcePermissionDto()
                    {
                        Name = g.Key,
                        Value = g.Select(rp => rp.Value)
                                            .ToList()
                                            .Aggregate((current, next) => current | next)
                    }).ToList();

                externalApplicationDto.ModulePermissions.Add(new ModulePermissoinDto()
                {
                    Module = module,
                    SectionPermissions = sectionPermissions,
                    ResourcePermissions = resourcePermissionsList
                });
            }

            return externalApplicationDto;
        }
    }
}
