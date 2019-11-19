using IdentityDatabase.Dto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Company.Common.Models.Identity;

namespace IdentityDatabase.Middlewares
{
    public class UserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Module _module;

        public UserMiddleware(RequestDelegate next, IOptions<UserMiddlewareConfiguration> options)
        {
            _next = next;
            _module = options.Value.Module;
        }

        public async Task InvokeAsync(HttpContext httpContext, IdentityContext context)
        {
            var jwtHandler = new JwtSecurityTokenHandler();

            var authorizationHeader = httpContext.Request.Headers["Authorization"].ToString();

            try
            {
                if (authorizationHeader == "")
                {
                    httpContext.Items.Add("User", null);
                }
                else
                {
                    var token = authorizationHeader.Split(' ')[1];

                    if (jwtHandler.CanReadToken(token))
                    {
                        var jwtToken = jwtHandler.ReadJwtToken(token);

                        var expJwtTokenTime = int.Parse(jwtToken.Claims.First(c => c.Type == "exp").Value);
                        var current_time = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

                        if (current_time > expJwtTokenTime)
                            throw new UnauthorizedAccessException();
                        else
                        {
                            int.TryParse(jwtToken.Claims.First(c => c.Type == "id").Value, out int id);

                            var user = context.Users.FirstOrDefault(u => u.Id == id);
                            var userDto = new UserDto()
                            {
                                Id = user.Id,
                                UserName = user.UserName,
                                Email = user.Email,
                                FirstName = user.FirstName,
                                DisplayName = user.DisplayName,
                                AuthorizationType = user.AuthorizationType,
                                LastName = user.LastName,
                                IsLead = user.IsLead,
                                DepartmentId = user.DepartmentId,
                                ProviderData = user.ProviderData
                            };

                            httpContext.Items.Add("User", userDto);
                        }
                    }
                }
            }
            catch
            {
                httpContext.Items.Add("User", null);
                httpContext.Response.StatusCode = 401;
                return;
            }

            await _next(httpContext);
        }
    }
}
