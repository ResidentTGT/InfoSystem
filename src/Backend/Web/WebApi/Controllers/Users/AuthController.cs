using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Company.Common.Helpers;
using NLog;
using Company.Common.Models.Identity;
using WebApi.Dto.Users;
using WebApi.Options;

namespace WebApi.Controllers.Users
{
    [Route("v1/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IdentityContext _dbContext;
        private readonly JwtFactory _jwtFactory;
        private readonly IConfiguration _configuration;
        private readonly Logger _logger;

        public AuthController(UserManager<User> userManager, IdentityContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _dbContext = context;
            _configuration = configuration;
            _jwtFactory = new JwtFactory();
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPost]
        public async Task<IActionResult> LoginLocal([FromBody]CredentialsDto credentials)
        {
            _logger.Debug("Start logining by credentials method...");
            _logger.Trace("Checking username and password...");
            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
            {
                _logger.Error("Incorrect username or password");
                return BadRequest("Неправильный логин или пароль.");
            }
            _logger.Trace($"Generating jwt token for user={credentials.UserName}...");
            var jwt = await _jwtFactory.GenerateEncodedToken(credentials.UserName, identity);
            _logger.Trace($"Jwt token successfully genereted for user={credentials.UserName}");

            var authResult = new AuthResultDto()
            {
                Token = jwt,
                UserId = Convert.ToInt32(identity.Claims.First(c => c.Type == IdentityDatabase.Helpers.JwtClaimIdentifiers.Id).Value)
            };
            _logger.Debug("Login by credentials method is finished");
            return Ok(authResult);
        }

        [HttpPost("msad/trassa")]
        public async Task<IActionResult> LoginMicrosoft([FromBody]MicrosoftAuthDto model)
        {
            _logger.Debug("Start logining through Active Directory method...");
            _logger.Trace("Getting AD config from configuration...");
            var adConfig = _configuration.GetSection("ActiveDirectory");
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, adConfig["StsDiscoveryEndPoint"], adConfig["TenantDomain"]);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = await configManager.GetConfigurationAsync();
            _logger.Trace("AD config successfully got");

            var tokenHandler = new JwtSecurityTokenHandler();
            _logger.Trace($"Creating token validations parameters...");
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = JwtOptions.TokenValidationParameters.ValidAudience,
                ValidIssuer = config.Issuer,
                IssuerSigningKeys = config.SigningKeys,
            };
            _logger.Trace($"Token validations parameters successfully created");
            _logger.Trace($"Generating jwt token...");
            JwtSecurityToken token;
            try
            {
                var validatedToken = (SecurityToken)new JwtSecurityToken();
                tokenHandler.ValidateToken(model.AccessToken, validationParameters, out validatedToken);
                token = tokenHandler.ReadToken(tokenHandler.WriteToken(validatedToken)) as JwtSecurityToken;
            }
            catch (Exception)
            {
                _logger.Error($"Incorrect Microsoft AD token");
                return BadRequest("Неправильный токен Microsoft AD.");
            }
            _logger.Trace($"Jwt token successfully genereted");
            _logger.Trace($"Generating Microsoft user data...");
            var userInfo = GenerateMicrosoftUserData(token);
            _logger.Trace($"Checking is user exist with mail={userInfo.Mail}...");
            var localUser = _dbContext.Users.FirstOrDefault(u => u.UserName == userInfo.Mail);
            if (localUser == null)
                try
                {
                    _logger.Trace($"Creating user with mail={userInfo.Mail}...");
                    localUser = await CreateUser(userInfo, JsonConvert.SerializeObject(token.Claims.ToDictionary(c => new KeyValuePair<string, string>(c.Type, c.Value))));
                    _logger.Trace($"User successfully created");
                }
                catch (Exception)
                {
                    _logger.Trace($"Couldn't create user");
                    return BadRequest("Не удалось создать локального пользователя.");
                }

            var authResult = new AuthResultDto()
            {
                Token = await _jwtFactory.GenerateEncodedToken(localUser.UserName, _jwtFactory.GenerateClaimsIdentity(localUser.UserName, localUser.Id)),
                UserId = localUser.Id
            };

            _logger.Debug("Login by credentials method is finished");
            return Ok(authResult);
        }

        private async Task<User> CreateUser(MicrosoftUserData userInfo, string token)
        {
            _logger.Trace("Creating user entity method...");
            var appUser = new User
            {
                ProviderData = JsonConvert.SerializeObject(new MicrosoftAdOptions() { Id = userInfo.Id, Token = token }),
                Email = userInfo.Mail,
                UserName = userInfo.Mail,
                DisplayName = userInfo.DisplayName,
                PasswordHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8)
            };
            _logger.Trace("User entity successfully created");

            await _dbContext.Users.AddAsync(appUser);
            _logger.Trace("Saving user to database ...");
            await _dbContext.SaveChangesAsync();
            _logger.Trace("User successfully saved to database");

            return appUser;
        }

        private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                return await Task.FromResult<ClaimsIdentity>(null);

            // get the user to verifty
            var userToVerify = await _userManager.FindByNameAsync(userName);

            if (userToVerify == null)
                return await Task.FromResult<ClaimsIdentity>(null);

            // check the credentials
            if (await _userManager.CheckPasswordAsync(userToVerify, password))
            {
                return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
            }

            // Credentials are invalid, or account doesn't exist
            return await Task.FromResult<ClaimsIdentity>(null);
        }

        private MicrosoftUserData GenerateMicrosoftUserData(JwtSecurityToken token)
        {
            var claims = token.Claims.ToDictionary(c => c.Type);
            return new MicrosoftUserData()
            {
                DisplayName = claims["name"].Value,
                Id = claims["oid"].Value,
                Mail = claims["preferred_username"].Value,
            };

        }
    }
}