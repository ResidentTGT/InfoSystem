using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityDatabase;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Company.Common.Models.Identity;
using Company.Users.Dto;
using Users.ExternalSystemsOptions;
using Users.ViewModels;

namespace Company.Users.Controllers
{
    [Route("v1/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IdentityContext _dbContext;
        private readonly JwtFactory _jwtFactory;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, IdentityContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _dbContext = context;
            _configuration = configuration;

            _jwtFactory = new JwtFactory();
        }

        [HttpPost]
        public async Task<IActionResult> LoginLocal([FromBody]CredentialsDto credentials)
        {
            var identity = await GetClaimsIdentity(credentials.UserName, credentials.Password);
            if (identity == null)
                return BadRequest("Неправильный логин или пароль.");

            var jwt = await _jwtFactory.GenerateEncodedToken(credentials.UserName, identity);

            var authResult = new AuthResultDto()
            {
                Token = jwt,
                UserId = Convert.ToInt32(identity.Claims.First(c => c.Type == IdentityDatabase.Helpers.JwtClaimIdentifiers.Id).Value)
            };

            return Ok(authResult);
        }

        // POST v1/auth/msad/trassa
        [HttpPost("msad/trassa")]
        public async Task<IActionResult> LoginMicrosoft([FromBody]MicrosoftAuthDto model)
        {
            var adConfig = _configuration.GetSection("ActiveDirectory");
            var stsDiscoveryEndpoint = String.Format(CultureInfo.InvariantCulture, adConfig["StsDiscoveryEndPoint"], adConfig["TenantDomain"]);
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            var config = await configManager.GetConfigurationAsync();

            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = JwtOptions.TokenValidationParameters.ValidAudience,
                ValidIssuer = config.Issuer,
                IssuerSigningKeys = config.SigningKeys,
            };

            JwtSecurityToken token;
            try
            {
                var validatedToken = (SecurityToken)new JwtSecurityToken();
                tokenHandler.ValidateToken(model.AccessToken, validationParameters, out validatedToken);
                token = tokenHandler.ReadToken(tokenHandler.WriteToken(validatedToken)) as JwtSecurityToken;
            }
            catch (Exception)
            {
                return BadRequest("Неправильный токен Microsoft AD.");
            }

            var userInfo = GenerateMicrosoftUserData(token);

            var localUser = _dbContext.Users.FirstOrDefault(u => u.UserName == userInfo.Mail);
            if (localUser == null)
                try
                {

                    localUser = await CreateUser(userInfo, JsonConvert.SerializeObject(token.Claims.ToDictionary(c => new KeyValuePair<string, string>(c.Type, c.Value))));
                }
                catch (Exception)
                {
                    return BadRequest("Не удалось создать локального пользователя.");
                }

            var authResult = new AuthResultDto()
            {
                Token = await _jwtFactory.GenerateEncodedToken(localUser.UserName, _jwtFactory.GenerateClaimsIdentity(localUser.UserName, localUser.Id)),
                UserId = localUser.Id
            };


            return Ok(authResult);
        }

        private async Task<User> CreateUser(MicrosoftUserData userInfo, string token)
        {
            var appUser = new User
            {
                FirstName = userInfo.GivenName,
                LastName = userInfo.Surname,
                ProviderData = JsonConvert.SerializeObject(new MicrosoftAdOptions() { Id = userInfo.Id, Token = token }),
                Email = userInfo.Mail,
                UserName = userInfo.Mail,
                DisplayName = $"{userInfo.GivenName} {userInfo.Surname}",
                PasswordHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8)
            };
            await _dbContext.Users.AddAsync(appUser);

            await _dbContext.SaveChangesAsync();

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
                GivenName = claims["given_name"].Value,
                Id = claims["oid"].Value,
                Mail = claims["unique_name"].Value,
                Surname = claims["family_name"].Value
            };
        }
    }
}