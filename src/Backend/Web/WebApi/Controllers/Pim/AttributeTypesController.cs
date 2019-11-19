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

namespace WebApi.Controllers.Pim
{
    [Route("v1/attributes/types")]
    [Authorize]
    public class AttributeTypesController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private  readonly Logger _logger;

        public AttributeTypesController(IHttpPimClient pimClient)
        {
            _pimClient = pimClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("lists")]
        [ProducesResponseType(typeof(List<ListDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetLists([FromQuery] bool withCategories)
        {
            _logger.Debug($"Start getting lists with categories={withCategories} method...");
            _logger.Trace($"Getting lists with categories={withCategories} from MS Pim...");
            var response = await _pimClient.AttributesTypes.GetLists(withCategories);
            _logger.Trace($"Lists with categories={withCategories} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ListDto...");
            var responseObject = await response.ResponseToDto<List<List>>((ll) => ll.Select(l => new ListDto(l)));
            _logger.Trace($"Response successfully converted to List of ListDto");
            _logger.Debug($"Getting lists with categories={withCategories} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }


        [HttpGet("lists/{id}")]
        [ProducesResponseType(typeof(ListDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetList([FromRoute] int id)
        {
            _logger.Debug($"Start getting list by id={id} method...");
            _logger.Trace($"Getting list by id={id} from MS Pim...");
            var response = await _pimClient.AttributesTypes.GetListById(id);
            _logger.Trace($"List by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to ListDto...");
            var responseObject = await response.ResponseToDto<List>(l => new ListDto(l));
            _logger.Trace($"Response successfully converted to ListDto");
            _logger.Debug($"Getting list by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }


        [HttpGet("list-values/{id}")]
        [ProducesResponseType(typeof(ListValueDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetListValue([FromRoute] int id)
        {
            _logger.Debug($"Start getting list value by id={id} method...");
            _logger.Trace($"Getting list value by id={id} from MS Pim...");
            var response = await _pimClient.AttributesTypes.GetListValueById(id);
            _logger.Trace($"List value by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to ListValueDto...");
            var responseObject = await response.ResponseToDto<ListValue>((l) => new ListValueDto(l));
            _logger.Trace($"Response successfully converted to ListValueDto");
            _logger.Debug($"Getting list value by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("listCmpyIds")]
        [ProducesResponseType(typeof(List<ListDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetListCmpyIds()
        {
            var listsIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start getting lists by ids={string.Join(",",listsIds)} method...");
            _logger.Trace($"Getting lists by ids={string.Join(",", listsIds)} from MS Pim...");
            var response = await _pimClient.AttributesTypes.GetListCmpyIds(listsIds);
            _logger.Trace($"Lists by ids={string.Join(",", listsIds)} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ListDto...");
            var responseObject = await response.ResponseToDto<List<List>>((ll) => ll.Select(l => new ListDto(l)));
            _logger.Trace($"Response successfully converted to List of ListDto");
            _logger.Debug($"Getting lists by ids={string.Join(",", listsIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }


        [HttpPost("lists")]
        [ProducesResponseType(typeof(ListDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateList([FromBody] ListDto listDto)
        {
            _logger.Debug($"Start creating list method...");
            _logger.Trace($"Creating list in MS Pim...");
            var response = await _pimClient.AttributesTypes.CreateList(listDto.ToEntity());
            _logger.Trace($"List successfully created in MS Pim");
            _logger.Trace($"Converting response to ListDto...");
            var responseObject = await response.ResponseToDto<List>(l => new ListDto(l));
            _logger.Trace($"Response successfully converted to ListDto");
            _logger.Debug($"Creating list method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
;
        }

        [HttpPost("listvalues")]
        [ProducesResponseType(typeof(ListValueDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateListValue([FromBody] ListValueDto listvalueDto)
        {
            _logger.Debug($"Start creating list value method...");
            _logger.Trace($"Creating list value in MS Pim...");
            var response = await _pimClient.AttributesTypes.CreateListValue(listvalueDto.ToEntity());
            _logger.Trace($"List value successfully created in MS Pim");
            _logger.Trace($"Converting response to ListValueDto...");
            var responseObject = await response.ResponseToDto<ListValue>((l) => new ListValueDto(l));
            _logger.Trace($"Response successfully converted to ListValueDto");
            _logger.Debug($"Creating list value method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("lists/{id}")]
        [ProducesResponseType(typeof(ListDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditList([FromRoute] int id, [FromBody] ListDto listDto)
        {
            _logger.Debug($"Start editing list by id={id} method...");
            _logger.Trace($"Editing list by id={id} in MS Pim...");
            var response = await _pimClient.AttributesTypes.EditList(id, listDto.ToEntity());
            _logger.Trace($"List with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to ListDto...");
            var responseObject = await response.ResponseToDto<List>((l) => new ListDto(l));
            _logger.Trace($"Response successfully converted to ListDto");
            _logger.Debug($"Editing list by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("listvalues/{id}")]
        [ProducesResponseType(typeof(ListValueDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditListValue([FromRoute] int id, [FromBody] ListValueDto listvalueDto)
        {
            _logger.Debug($"Start editing list value by id={id} method...");
            _logger.Trace($"Editing list value by id={id} in MS Pim...");
            var response = await _pimClient.AttributesTypes.EditListValue(id, listvalueDto.ToEntity());
            _logger.Trace($"List value with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to ListValueDto...");
            var responseObject = await response.ResponseToDto<ListValue>((l) => new ListValueDto(l));
            _logger.Trace($"Response successfully converted to ListValueDto");
            _logger.Debug($"Editing list value by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("lists/{id}")]
        [ProducesResponseType(typeof(ListDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteList([FromRoute] int id)
        {
            _logger.Debug($"Start deleting list by id={id} method...");
            _logger.Trace($"Deleting list by id={id} in MS Pim...");
            var response = await _pimClient.AttributesTypes.DeleteList(id);
            _logger.Trace($"List with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to ListDto...");
            var responseObject = await response.ResponseToDto<List>((l) => new ListDto(l));
            _logger.Trace($"Response successfully converted to ListDto");
            _logger.Debug($"Deleting list by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("listvalues/{id}")]
        [ProducesResponseType(typeof(ListValueDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteListValue([FromRoute] int id)
        {
            _logger.Debug($"Start deleting list value by id={id} method...");
            _logger.Trace($"Deleting list value by id={id} in MS Pim...");
            var response = await _pimClient.AttributesTypes.DeleteListValue(id);
            _logger.Trace($"List value with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to ListValueDto...");
            var responseObject = await response.ResponseToDto<ListValue>((l) => new ListValueDto(l));
            _logger.Trace($"Response successfully converted to ListValueDto");
            _logger.Debug($"Deleting list value by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}