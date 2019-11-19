using IdentityDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Company.Common.Models.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;

        public UserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext, IdentityContext context)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

            try
            {
                if (authorizationHeader == "")
                    httpContext.Items.Add("User", null);
                else
                {
                    var token = authorizationHeader.Split(' ')[1];

                    if (jwtHandler.CanReadToken(token))
                    {
                        var jwtToken = jwtHandler.ReadJwtToken(token);

                        var expJwtTokenTime = int.Parse(jwtToken.Claims.First(c => c.Type == "exp").Value);
                        var current_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                        if (current_time > expJwtTokenTime)
                            httpContext.Items.Add("User", null);
                        else
                        {
                            int.TryParse(jwtToken.Claims.First(c => c.Type == "id").Value, out int id);

                            var user = context.Users
                                            .Include(u => u.UserRoles)
                                            .ThenInclude(ur => ur.Role.ResourcePermissions)
                                            .Include(u => u.UserRoles)
                                            .ThenInclude(ur => ur.Role.SectionPermissions)
                                            .Single(u => u.Id == id);

                            httpContext.Items.Add("User", user);
                        }
                    }
                }
            }
            catch
            {
                httpContext.Items.Add("User", null);
            }

            await _next(httpContext);
        }
    }
}
