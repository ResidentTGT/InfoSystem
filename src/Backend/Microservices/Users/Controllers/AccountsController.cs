using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Company.Common.Models.Identity;
using Company.Users.Dto;
using Users.ViewModels;

namespace Company.Users.Controllers
{
    [Route("v1/accounts")]
    public class AccountsController : Controller
    {
        private readonly IdentityContext _dbContext;
        private readonly UserManager<User> _userManager;

        public AccountsController(UserManager<User> userManager, IdentityContext context)
        {
            _userManager = userManager;
            _dbContext = context;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]RegistrationDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new User()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                // UserName is UNIQUE in Identity Db
                UserName = model.UserName,
                DisplayName = model.FirstName != "" && model.LastName != "" ? $"{model.FirstName} {model.LastName}" : model.UserName,
                AuthorizationType = AuthorizationType.Local
            };

            if (_dbContext.Users.Any(u => u.UserName == user.UserName) == false)
            {
                var result = await _userManager.CreateAsync(user, model.Password);

                await _dbContext.SaveChangesAsync();
            }
            else
            {
                return BadRequest("Не удалось создать пользователя. Пользователь с таким именем уже существует.");
            }

            return Ok();
        }

        // GET api/accounts/5
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Get(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userViewModel = new UserDto()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Roles = new List<string>()
            };

            return Ok(userViewModel);
        }
    }

}