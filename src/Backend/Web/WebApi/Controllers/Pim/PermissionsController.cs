using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.Common.Models.Users;
using Company.Common.Models.Pim;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Dto.Users;
using WebApi.Extensions;
using RoleDto = WebApi.Dto.Pim.RoleDto;
using WebApi.Attributes;
using Company.Common.Models.Identity;
using IdentityDatabase;
using Microsoft.EntityFrameworkCore;
using NLog;
using ResourcePermission = Company.Common.Models.Users.ResourcePermission;
using SectionPermission = Company.Common.Models.Users.SectionPermission;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class PermissionsController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly IdentityContext _identityContext;
        private readonly Logger _logger;

        public PermissionsController(IHttpPimClient pimClient, IdentityContext identityContext)
        {
            _pimClient = pimClient;
            _identityContext = identityContext;
            _logger = LogManager.GetCurrentClassLogger();
        }


        [HttpGet("roles")]
        [ProducesResponseType(typeof(List<RoleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetRoles([FromQuery] Module module)
        {
            _logger.Debug($"Start getting roles method...");
            _logger.Debug($"Getting roles...");
            var roles = _identityContext.Roles.Where(r => r.Module == module).OrderBy(c => c.Name).ToList()
                .Select(re => new RoleDto(re, _identityContext.SectionPermissions.Where(sp => sp.RoleId == re.Id).ToList()));
            _logger.Debug($"Roles successfully received");
            _logger.Debug($"Start getting roles method is finished...");
            return StatusCode(200, roles);
        }

        [HttpGet("user-roles")]
        [ProducesResponseType(typeof(List<RoleDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetUserRoles([FromQuery] Module module)
        {
            _logger.Debug($"Start getting roles method...");         
            var user = HttpContext.Items["User"] as User;
            _logger.Debug($"Getting user roles by userId={user?.Id}...");
            var userRoles = _identityContext.Roles.Where(r => r.Module == module && r.UserRoles.Select(ur => ur.UserId).Contains(user.Id)).OrderBy(r => r.Name).ToList()
                .Select(re => new RoleDto(re, _identityContext.SectionPermissions.Where(sp => sp.RoleId == re.Id).ToList()));
            _logger.Debug($"User roles by userId={user?.Id} successfully received");
            _logger.Debug($"Start getting user roles by userId={user?.Id} method is finished...");
            return StatusCode(200, userRoles);
        }

        [HttpGet("resources/names")]
        [ProducesResponseType(typeof(List<string>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetResourcesNames()
        {
            _logger.Debug($"Start getting resources names method...");
            _logger.Debug($"Getting resources names from MS Pim...");
            var response = await _pimClient.Permissions.GetResourcesNames();
            _logger.Debug($"Resources names from MS Pim successfully received ");
            _logger.Trace($"Converting response to string...");
            var responseObject = await response.ResponseToDto<List<string>>();
            _logger.Trace($"Response successfully converted to string");
            _logger.Debug($"Getting resources names method is finished...");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("resources/{roleId}")]
        [ProducesResponseType(typeof(List<PimResourcePermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetResourcesPermissionCmpyRole([FromRoute] int roleId)
        {
            _logger.Debug($"Start getting resources permissions by roleId={roleId} method...");
            _logger.Debug($"Getting resources permissions by roleId={roleId}...");
            var resourcesPermissions = await _identityContext.ResourcePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
            _logger.Debug($"Resources permissions successfully received ");
            _logger.Debug($"Getting resources permissions by roleId={roleId} method is finished");
            return StatusCode(200, resourcesPermissions);
        }

        [HttpGet("attributes/{userId}")]
        [ProducesResponseType(typeof(List<AttributePermissionDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAttributesPermissionCmpyUserId([FromRoute] int userId)
        {
            _logger.Debug($"Start getting attributes permissions by userId={userId} method...");
            _logger.Debug($"Getting attributes permissions by userId={userId} from MS Pim...");
            var response = await _pimClient.Permissions.GetAttributesPermissionCmpyUserId(userId);
            _logger.Debug($"Attributes permissions from MS Pim successfully received ");
            _logger.Trace($"Converting response to List of AttributePermissionDto...");
            var responseObject = await response.ResponseToDto<List<AttributePermission>>((lap => lap.Select(ap => new AttributePermissionDto(ap))));
            _logger.Trace($"Response successfully converted to List of AttributePermissionDto");
            _logger.Debug($"Getting attributes permissions by userId={userId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost("sections")]
        [ProducesResponseType(typeof(SectionPermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateSectionPermission([FromBody] SectionPermissionDto sectionPermissionDto)
        {
            _logger.Debug($"Start creating section permission method...");
            _logger.Trace($"Getting section permission by Name={sectionPermissionDto.Name} and RoleId={sectionPermissionDto.RoleId}...");
            var sectionPermission = await _identityContext.SectionPermissions.FirstOrDefaultAsync(rp => rp.Name == sectionPermissionDto.Name && rp.RoleId == sectionPermissionDto.RoleId);
            _logger.Trace($"Section permission from MS Pim successfully received ");
            if (sectionPermission == null)
            {
                _logger.Trace($"Creating Section permission entity...");
                sectionPermission = new SectionPermission() { Name = sectionPermissionDto.Name, RoleId = sectionPermissionDto.RoleId };
                _identityContext.Add(sectionPermission);
                _logger.Trace($"Saving section permission ...");
                await _identityContext.SaveChangesAsync();
                _logger.Trace($"Section permission successfully saved");
                _logger.Debug($"Creating section permission method is finished");
                return StatusCode(200, new SectionPermissionDto() { Id = sectionPermission.Id, Name = sectionPermission.Name, RoleId = sectionPermission.RoleId });
            }
            else
            {
                _logger.Error($"Section permissions with Name={sectionPermissionDto.Name} and RoleId={sectionPermissionDto.RoleId} is already exist");
                return BadRequest($"Section permission со значениями 'Name': '{sectionPermissionDto.Name}' и 'RoleId': {sectionPermissionDto.RoleId} уже существует.");
            }
        }

        [HttpPost("resources")]
        [ProducesResponseType(typeof(PimResourcePermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateResourcePermission([FromBody] PimResourcePermissionDto resourcePermissionDto)
        {
            _logger.Debug($"Start creating resource permission method...");
            _logger.Trace($"Getting resource permission by Name={resourcePermissionDto.Name} and RoleId={resourcePermissionDto.RoleId}...");

            var resourcePermission = await _identityContext.ResourcePermissions.FirstOrDefaultAsync(rp => rp.Name == resourcePermissionDto.Name && rp.RoleId == resourcePermissionDto.RoleId);
            _logger.Trace($"Resource permission from MS Pim successfully received ");

            if (resourcePermission == null)
            {
                _logger.Trace($"Creating resource permission entity...");
                resourcePermission = new ResourcePermission() { Name = resourcePermissionDto.Name, RoleId = resourcePermissionDto.RoleId, Value = resourcePermissionDto.Value };
                _identityContext.Add(resourcePermission);
                _logger.Trace($"Saving resource permission ...");
                await _identityContext.SaveChangesAsync();
                _logger.Trace($"Resource permission successfully saved");
                _logger.Debug($"Creating resource permission method is finished");
                return StatusCode(200, new PimResourcePermissionDto() { Id = resourcePermission.Id, Name = resourcePermission.Name, RoleId = resourcePermission.RoleId, Value = resourcePermission.Value });
            }
            else
            {
                _logger.Error($"Resource permissions with Name={resourcePermissionDto.Name} and RoleId={resourcePermissionDto.RoleId} is already exist");
                return BadRequest($"Resource permission со значениями 'Name': '{resourcePermissionDto.Name}' и 'RoleId': {resourcePermissionDto.RoleId} уже существует.");
            }
        }

        [HttpPut("resources/{id}")]
        [ProducesResponseType(typeof(PimResourcePermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditResourcePermission([FromRoute] int id, [FromBody] PimResourcePermissionDto resourcePermissionDto)
        {
            _logger.Debug($"Start editing resource permission by id={id} method...");
            _logger.Trace($"Getting resource permission by id={id}...");
            var resourcePermission = await _identityContext.ResourcePermissions.FindAsync(id);
            ;
            if (resourcePermission == null)
            {
                _logger.Error($"Resource permission with id={id} not found");
                return NotFound();
            }

            if (_identityContext.ResourcePermissions.Any(rp => rp.Id != id && rp.Name == resourcePermission.Name && rp.RoleId == resourcePermissionDto.RoleId))
            {
                _logger.Error($"Resource permissions with Name={resourcePermissionDto.Name} and RoleId={resourcePermissionDto.RoleId} is already exist");
                return BadRequest($"Resource permission with same 'Name': '{resourcePermission.Name}' already exist.");
            }
            _logger.Trace($"Editing resource permission ...");
            resourcePermission.Name = resourcePermissionDto.Name;
            resourcePermission.Value = resourcePermissionDto.Value;
            _logger.Trace($"Saving resource permission ...");
            await _identityContext.SaveChangesAsync();
            _logger.Trace($"Resource permission successfully saved");
            _logger.Debug($"Editing resource permission by id={id} method is finished");
            return StatusCode(200, new PimResourcePermissionDto() { Id = resourcePermission.Id, Name = resourcePermission.Name, RoleId = resourcePermission.RoleId, Value = resourcePermission.Value });
        }

        [HttpDelete("sections/{id}")]
        [ProducesResponseType(typeof(SectionPermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteSectionPermission([FromRoute] int id)
        {
            _logger.Debug($"Start editing ressectionource permission by id={id} method...");
            _logger.Trace($"Getting section permission by id={id}...");
            var sectionPermissionEntity = await _identityContext.SectionPermissions.FindAsync(id);

            if (sectionPermissionEntity == null)
            {
                _logger.Error($"Section permission with id={id} not found");
                return NotFound();
            }
            _logger.Trace($"Deleting section permission ...");
            _identityContext.Remove(sectionPermissionEntity);
            await _identityContext.SaveChangesAsync();
            _logger.Trace($"Section permission successfully deleted");
            _logger.Debug($"Deleting section permission by id={id} method is finished");
            return StatusCode(200, new SectionPermissionDto() { Id = sectionPermissionEntity.Id, Name = sectionPermissionEntity.Name, RoleId = sectionPermissionEntity.RoleId });
        }

        [HttpDelete("resources/{id}")]
        [ProducesResponseType(typeof(PimResourcePermissionDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteResourcePermission([FromRoute] int id)
        {
            _logger.Debug($"Start deleting resource permission by id={id} method...");
            _logger.Trace($"Getting resource permission by id={id}...");
            var resourcePermissionEntity = await _identityContext.ResourcePermissions.FirstOrDefaultAsync(sp => sp.Id == id);

            if (resourcePermissionEntity == null)
            {
                _logger.Error($"Resource permission with id={id} not found");
                return NotFound();
            }
            _logger.Trace($"Deleting resource permission ...");
            _identityContext.Remove(resourcePermissionEntity);
            await _identityContext.SaveChangesAsync();
            _logger.Trace($"Resource permission successfully deleted");
            _logger.Debug($"Deleting resource permission by id={id} method is finished");
            return StatusCode(200, new PimResourcePermissionDto() { Id = resourcePermissionEntity.Id, Name = resourcePermissionEntity.Name, RoleId = resourcePermissionEntity.RoleId });
        }
    }
}