using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Identity;
using WebApi.Dto.Users;

namespace WebApi.Controllers.Users
{
    [Route("v1/accounts")]
    public class AccountsController : Controller
    {
        private readonly IdentityContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly Logger _logger;

        public AccountsController(UserManager<User> userManager, IdentityContext context)
        {
            _userManager = userManager;
            _dbContext = context;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody]RegistrationDto model)
        {
            _logger.Debug("Start registering users method...");
            _logger.Trace("Validating user data...");
            if (!ModelState.IsValid)
            {
                _logger.Error("Error on validate user data");
                return BadRequest(ModelState);
            }
            _logger.Trace("User data successfully validated");
            _logger.Trace("Creating user entity...");
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
            _logger.Trace("User entity successfully created");

            _logger.Trace("Checking username for uniqueness...");
            if (_dbContext.Users.Any(u => u.UserName == user.UserName) == false)
            {
                _logger.Trace("Creating user...");
                var result = await _userManager.CreateAsync(user, model.Password);
                _logger.Trace("User successfully created");
                _logger.Trace("Saving user to database ...");
                await _dbContext.SaveChangesAsync();
                _logger.Trace("User successfully saved to database");
            }
            else
            {
                _logger.Error($"User with same UserName={user.UserName} is already registered");
                return BadRequest("Не удалось создать пользователя. Пользователь с таким именем уже существует.");
            }
            _logger.Debug("Register user method is finished");
            return Ok();
        }
    }
}