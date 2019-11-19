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
    public class AttributesCategoriesController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly Logger _logger;

        public AttributesCategoriesController(IHttpPimClient pimClient)
        {
            _pimClient = pimClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<AttributeCategory>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery] int categoryId)
        {
            _logger.Debug($"Start getting attribute category by categoryId={categoryId} method...");
            _logger.Trace($"Getting attribute category by categoryId={categoryId} from MS Pim...");
            var response = await _pimClient.AttributesCategories.GetById(categoryId);
            _logger.Trace($"Attribute category by categoryId={categoryId} from MS Pim successfully received");
            _logger.Trace($"Converting response to AttributeCategory...");
            var responseObject = await response.ResponseToDto<List<AttributeCategory>>();
            _logger.Trace($"Response successfully converted to AttributeCategory");
            _logger.Debug($"Getting attribute category by categoryId={categoryId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{categoryId}")]
        [ProducesResponseType(typeof(List<AttributeCategory>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int categoryId, [FromBody] List<AttributeCategoryDto> attributeCategoriesDto)
        {
            _logger.Debug($"Start editing attribute category by categoryId={categoryId} method...");
            _logger.Trace($"Editing attribute category by categoryId={categoryId} in MS Pim...");
            var response = await _pimClient.AttributesCategories.Edit(categoryId, attributeCategoriesDto.Select(acd => acd.ToEntity()).ToList());
            _logger.Trace($"Attribute category by categoryId={categoryId} successfully edited in MS Pim");
            _logger.Trace($"Converting response to AttributeCategory...");
            var responseObject = await response.ResponseToDto<List<AttributeCategory>>();
            _logger.Trace($"Response successfully converted to AttributeCategory");
            _logger.Debug($"Editing attribute category by categoryId={categoryId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}