using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Pim.Client.v2;
using NLog;

namespace Company.Pim.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/[controller]")]
    public class PermissionsController : Controller
    {
        private const Module CurrentModule = Module.PIM;
        private IWebApiCommunicator _httpWebApiCommunicator { get; set; }

        private readonly PimContext _pimContext;
        private readonly Logger _logger;

        public PermissionsController(PimContext pimContext, IWebApiCommunicator httpWebApiCommunicator)
        {
            _pimContext = pimContext;
            _httpWebApiCommunicator = httpWebApiCommunicator;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("resources/names")]
        public async Task<IActionResult> GetResourcesNames() => Ok(PimResourcePermissions.GetPermissionsNames().OrderBy(c => c).ToArray());

        [HttpGet("attributes/{userId}")]
        public async Task<IActionResult> GetAttributesPermissionCmpyUserId([FromRoute] int userId)
        {
            _logger.Log(LogLevel.Debug, "Start getting attributes permissions...");
           
            User user = null;
            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, $"Getting user roles of the user '{user.UserName}'...");
            var userRoles = user.UserRoles
                .Where(ur => ur.Role.Module == CurrentModule)
                .ToList();

            if (!userRoles.Any())
            {
                _logger.Log(LogLevel.Error, $"User roles of the user '{user.UserName}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"User roles of the user '{user.UserName}' successfully received");

            _logger.Log(LogLevel.Trace, "Getting attribute permissions groups...");
            var attributePermissionsGroups = _pimContext.AttributePermissions.Include(ap => ap.Attribute)
                .Where(ap => ap.Attribute.DeleteTime == null
                             && userRoles.Select(ur => ur.RoleId).Contains(ap.RoleId))
                .GroupBy(ap => ap.AttributeId);
            _logger.Log(LogLevel.Trace, "Attribute permissions groups successfully received");

            _logger.Log(LogLevel.Trace, "Updating attribute permissions groups...");
            var attributePermissionsList = attributePermissionsGroups.Select(g => new AttributePermission()
            {
                AttributeId = g.First().AttributeId
            })
                .ToList();
            
            foreach (var ap in attributePermissionsList)
            {
                ap.Value = attributePermissionsGroups.First(g => g.First().AttributeId == ap.AttributeId)
                    .Select(r => r.Value)
                    .Aggregate((cur, next) => cur | next);
            }
            _logger.Log(LogLevel.Trace, "Attribute permissions groups are updated");

            _logger.Log(LogLevel.Debug, "Getting attributes permissions is finished.");
            return Ok(attributePermissionsList);
        }

    }
}