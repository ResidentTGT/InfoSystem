using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Pim;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Extensions;
using Attribute = Company.Common.Models.Pim.Attribute;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class AttributesController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly Logger _logger;

        public AttributesController(IHttpPimClient pimClient)
        {
            _pimClient = pimClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AttributeDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery] bool withCategories, [FromQuery] bool withPermissions)
        {
            _logger.Debug($"Start getting attributes with categories={withCategories} method...");
            _logger.Trace($"Getting attributes with categories={withCategories} from MS Pim...");
            var response = await _pimClient.Attributes.Get(withCategories, withPermissions);
            _logger.Trace($"Attributes with categories={withCategories} from MS Pim successfully received");
            _logger.Trace($"Converting response to AttributeDto...");
            var responseObject = await response.ResponseToDto<List<Attribute>>((la) => la.Select(a => new AttributeDto(a, true)));
            _logger.Trace($"Response successfully converted to AttributeDto");
            _logger.Debug($"Getting attributes with categories={withCategories} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AttributeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            _logger.Debug($"Start getting attribute by id={id} method...");
            _logger.Trace($"Getting attribute by id={id} from MS Pim...");
            var response = await _pimClient.Attributes.GetById(id);
            _logger.Trace($"Attribute by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to AttributeDto...");
            var responseObject = await response.ResponseToDto<AttributeResponseDto>((ar) => new AttributeDto(ar.Attribute, ar.WithCategories));
            _logger.Trace($"Response successfully converted to AttributeDto");
            _logger.Debug($"Getting attribute by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
   
        }

        [HttpGet("groups/{id}")]
        [ProducesResponseType(typeof(AttributeGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            _logger.Debug($"Start getting group by id={id} method...");
            _logger.Trace($"Getting group by id={id} from MS Pim...");
            var response = await _pimClient.Attributes.GetGroupById(id);
            _logger.Trace($"Group by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<AttributeGroup>((ag) => new AttributeGroupDto(ag));
            _logger.Trace($"Response successfully converted to AttributeGroupDto");
            _logger.Debug($"Getting group by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
        [HttpGet("groups/")]
        [ProducesResponseType(typeof(List<AttributeGroup>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetGroups()
        {
            _logger.Debug($"Start getting groups method...");
            _logger.Trace($"Getting groups from MS Pim...");
            var response = await _pimClient.Attributes.GetGroups();
            _logger.Trace($"Groups from MS Pim successfully received");
            _logger.Trace($"Converting response to List of AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<List<AttributeGroup>>((agl) => agl.Select(ag => new AttributeGroupDto(ag)));
            _logger.Trace($"Response successfully converted to List of AttributeGroupDto");
            _logger.Debug($"Getting groups method is finished");
            return StatusCode((int)response.StatusCode,responseObject );
        }
        [HttpPost]
        [ProducesResponseType(typeof(AttributeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Create([FromBody] AttributeDto attributeDto)
        {
            _logger.Debug($"Start creating attribute method...");
            _logger.Trace($"Creating attribute in MS Pim...");
            var response = await _pimClient.Attributes.Create(attributeDto.ToEntity());
            _logger.Trace($"Attribute successfully created in MS Pim");
            _logger.Trace($"Converting response to AttributeDto...");
            var responseObject = await response.ResponseToDto<AttributeResponseDto>((ar) => new AttributeDto(ar.Attribute, ar.WithCategories));
            _logger.Trace($"Response successfully converted to AttributeDto");
            _logger.Debug($"Creating attribute method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost("groups")]
        [ProducesResponseType(typeof(AttributeGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateGroup([FromBody] AttributeGroupDto attributeGroupDto)
        {
            _logger.Debug($"Start creating group method...");
            _logger.Trace($"Creating group in MS Pim...");
            var response = await _pimClient.Attributes.CreateGroup(attributeGroupDto.ToEntity());
            _logger.Trace($"Group successfully created in MS Pim");
            _logger.Trace($"Converting response to AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<AttributeGroup>((ag) => new AttributeGroupDto(ag));
            _logger.Trace($"Response successfully converted to AttributeGroupDto");
            _logger.Debug($"Creating group method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AttributeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] AttributeDto attributeDto)
        {
            _logger.Debug($"Start editing attribute by id={id} method...");
            _logger.Trace($"Editing attribute by id={id} in MS Pim...");
            var response = await _pimClient.Attributes.Edit(id, attributeDto.ToEntity());
            _logger.Trace($"Attribute with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to AttributeDto...");
            var responseObject = await response.ResponseToDto<AttributeResponseDto>((ar) => new AttributeDto(ar.Attribute, ar.WithCategories));
            _logger.Trace($"Response successfully converted to AttributeDto");
            _logger.Debug($"Editing attribute by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("groups/{id}")]
        [ProducesResponseType(typeof(AttributeGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditGroup([FromRoute] int id, [FromBody] AttributeGroupDto attributeGroupDto)
        {
            _logger.Debug($"Start editing group by id={id} method...");
            _logger.Trace($"Editing group by id={id} in MS Pim...");
            var response = await _pimClient.Attributes.EditGroup(id, attributeGroupDto.ToEntity());
            _logger.Trace($"Group with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<AttributeGroup>((ag) => new AttributeGroupDto(ag));
            _logger.Trace($"Response successfully converted to AttributeGroupDto");
            _logger.Debug($"Editing group by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(AttributeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.Debug($"Start deleting attribute by id={id} method...");
            _logger.Trace($"Deleting attribute by id={id} in MS Pim...");
            var response = await _pimClient.Attributes.DeleteById(id);
            _logger.Trace($"Attribute with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to AttributeDto...");
            var responseObject = await response.ResponseToDto<AttributeResponseDto>((ar) => new AttributeDto(ar.Attribute, ar.WithCategories));
            _logger.Trace($"Response successfully converted to AttributeDto");
            _logger.Debug($"Deleting attribute by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(List<AttributeDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteAttributes()
        {
            var attributeIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start deleting attributes by ids={string.Join(",", attributeIds)} method...");
            _logger.Trace($"Deleting attributes by ids={string.Join(",", attributeIds)} in MS Pim...");
            var response = await _pimClient.Attributes.DeleteByIds(attributeIds);
            _logger.Trace($"Attributes with ids={string.Join(",", attributeIds)} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to List of AttributeDto...");
            var responseObject = await response.ResponseToDto<List<Attribute>>((la) => la.Select(a => new AttributeDto(a, true)));
            _logger.Trace($"Response successfully converted to List of AttributeDto");
            _logger.Debug($"Deleting attributes by ids={string.Join(",", attributeIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("groups/{id}")]
        [ProducesResponseType(typeof(AttributeGroupDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteGroup([FromRoute] int id)
        {
            _logger.Debug($"Start deleting group by id={id} method...");
            _logger.Trace($"Deleting group by id={id} in MS Pim...");
            var response = await _pimClient.Attributes.DeleteGroupById(id);
            _logger.Trace($"Group with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<AttributeGroup>((ag) => new AttributeGroupDto(ag));
            _logger.Trace($"Response successfully converted to AttributeGroupDto");
            _logger.Debug($"Deleting group by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("groups")]
        [ProducesResponseType(typeof(List<AttributeGroupDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteGroups()
        {
            var groupsIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start deleting groups by ids={string.Join(",", groupsIds)} method...");
            _logger.Trace($"Deleting groups by ids={string.Join(",", groupsIds)} in MS Pim...");
            var response = await _pimClient.Attributes.DeleteGroupByIds(groupsIds);
            _logger.Trace($"Groups with ids={string.Join(",", groupsIds)} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to List of AttributeGroupDto...");
            var responseObject = await response.ResponseToDto<List<AttributeGroup>>((agl) => agl.Select(ag => new AttributeGroupDto(ag)));
            _logger.Trace($"Response successfully converted to List of AttributeGroupDto");
            _logger.Debug($"Deleting groups by ids={string.Join(",", groupsIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("filters")]
        [ProducesResponseType(typeof(List<Attribute>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetFilteredAttributes([FromQuery] int categoryId)
        {
            _logger.Debug($"Start getting filtered attributes by categoryId={categoryId} method...");
            _logger.Trace($"Getting filtered attributes by categoryId={categoryId} from MS Pim...");
            var response = await _pimClient.Attributes.GetFilteredAttributes(categoryId);
            _logger.Trace($"Filtered attributes by categoryId={categoryId} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of AttributeDto...");
            var responseObject = await response.ResponseToDto<List<Attribute>>((la) => la.Select(a => new AttributeDto(a, false)));
            _logger.Trace($"Response successfully converted to List of AttributeDto");
            _logger.Debug($"Getting filtered attributes by categoryId={categoryId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}