using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IdentityDatabase.Dto;
using Company.Common.Models.Identity;

namespace Users.Controllers
{
    [Route("v1/ExternalApplication")]
    public class ExternalApplicationController : Controller
    {
        private IdentityContext _context { get; set; }

        public ExternalApplicationController(IdentityContext context)
        {
            _context = context;
        }

        // POST: v1/ExternalApplication/register
        [HttpPost]
        [Authorize(Policy = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]ExternalApplicationDto extApp)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ExternalApplication app;
            if (_context.ExternalApplications.Any(a => a.Name == extApp.Name) == false)
            {
                app = new ExternalApplication()
                {
                    ApiKey = Guid.NewGuid().ToString(),
                    Name = extApp.Name
                };

                var result = await _context.ExternalApplications.AddAsync(app);

                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Failed to register new application. Application with this name already existed.");
            }

            return new OkObjectResult(app);
        }
    }
}
