using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using IdentityDatabase.Dto;
using Microsoft.Extensions.Options;

using Company.Common.Models.Identity;

namespace IdentityDatabase.Middlewares
{
    public class UserAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Module _module;
        //IdentityContext _context;
        //IPermissionDbContext _permissions;

        public UserAuthenticationMiddleware(RequestDelegate next, IOptions<UserMiddlewareConfiguration> options)
        {
            _next = next;
            _module = options.Value.Module;
            //_context = context;
            //_permissions = permissions;
        }

        public async Task InvokeAsync(HttpContext httpContext, IdentityContext context, IPermissionDbContext permissions)
        {
            var userDto = httpContext.Items["User"] as UserDto;

            if (userDto != null)
            {
                httpContext.Items["User"] = GetUserWithPermissionsDto(userDto, context, permissions);   
            }
            
            await _next(httpContext);
        }

        private UserDto GetUserWithPermissionsDto(UserDto userDto, IdentityContext context, IPermissionDbContext permissions = null)
        {
            var roles = context.Roles.Include(r => r.UserRoles).Where(r => r.UserRoles.Select(ur => ur.UserId).Contains(userDto.Id)).ToList();

            foreach (var module in roles.GroupBy(r => r.Module).Select(g => g.Select(r => r.Module).First()).ToList())
            {
                var sectionPermissions = permissions.SectionPermissions
                    .Where(sp => roles.Where(r => r.Module == module).Select(r => r.Id).Contains(sp.RoleId)).GroupBy(sp => sp.Name).Select(g => g.FirstOrDefault()).ToList();

                var resourcePermissionsList = permissions.ResourcePermissions
                    .Where(rp => roles.Where(r => r.Module == module).Select(r => r.Id).Contains(rp.RoleId))
                    .GroupBy(rp => rp.Name)
                    .ToList()
                    .Select(g => new ResourcePermissionDto()
                    {
                        Name = g.Key, 
                        Value = g.Select(rp => rp.Value)
                                            .ToList()
                                            .Aggregate((current, next) => current | next)
                    }).ToList();

                userDto.ModulePermissions.Add(new ModulePermissoinDto()
                {
                    Module = module,
                    SectionPermissions = sectionPermissions,
                    ResourcePermissions = resourcePermissionsList
                });
            }

            userDto.Roles.AddRange(roles.Select(r => new RoleDto(r)));

            return userDto;
        }
    }
}
