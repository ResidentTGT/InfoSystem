using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Pim;
using Company.Common.Models.Users;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Extensions;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class CategoriesController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly Logger _logger;

        public CategoriesController(IHttpPimClient pimClient)
        {
            _pimClient = pimClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            _logger.Debug($"Start getting category by id={id} method...");
            _logger.Trace($"Getting category by id={id} from MS Pim...");
            var response = await _pimClient.Categories.Get(id);
            _logger.Trace($"Category by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to CategoryDto...");
            var responseObject = await response.ResponseToDto<Category>(c => new CategoryDto(c));
            _logger.Trace($"Response successfully converted to CategoryDto");
            _logger.Debug($"Getting category by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<Category>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get()
        {
            _logger.Debug($"Start getting categories method...");
            _logger.Trace($"Getting categories from MS Pim...");
            var response = await _pimClient.Categories.Get();
            _logger.Trace($"Categories  from MS Pim successfully received");
            _logger.Trace($"Converting response to List of CategoryDto...");
            var responseObject = await response.ResponseToDto<List<Category>>(lc => lc.Select(c => new CategoryDto(c, true)));
            _logger.Trace($"Response successfully converted to List of CategoryDto");
            _logger.Debug($"Getting categories method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

        }

        [HttpGet("attributes")]
        [ProducesResponseType(typeof(List<AttributeDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAttributeCmpyCategoriesIdsFromCalculator()
        {
            var categoriesIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start getting categories by ids={string.Join(",", categoriesIds)} method...");
            _logger.Trace($"Getting categories by ids={string.Join(",", categoriesIds)} from MS Pim...");
            var response = await _pimClient.Categories.GetAttributeCmpyCategoriesIdsFromCalculator(categoriesIds);
            _logger.Trace($"Categories by ids={string.Join(",", categoriesIds)} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of AttributeDto...");
            var responseObject = await response.ResponseToDto<List<Attribute>>(a => a.Select(attr => new AttributeDto(attr)));
            _logger.Trace($"Response successfully converted to List of AttributeDto");
            _logger.Debug($"Getting categories by ids={string.Join(",", categoriesIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("{id}/attributes")]
        [ProducesResponseType(typeof(List<AttributeDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAttributes(int id)
        {
            _logger.Debug($"Start getting attributes by categoryId={id} method...");
            _logger.Trace($"Getting attributes by categoryId={id} from MS Pim...");
            var response = await _pimClient.Categories.GetAttributes(id);
            _logger.Trace($"Attributes by categoryId={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of AttributeDto...");
            var responseObject = await response.ResponseToDto<List<Attribute>>(a => a.Select(attr => new AttributeDto(attr)));
            _logger.Trace($"Response successfully converted to List of AttributeDto");
            _logger.Debug($"Getting attributes by categoryId={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Create([FromBody] CategoryDto categoryDto)
        {
            _logger.Debug($"Start creating category method...");
            _logger.Trace($"Creating category in MS Pim...");
            var response = await _pimClient.Categories.Create(categoryDto.ToEntity());
            _logger.Trace($"Category successfully created in MS Pim");
            _logger.Trace($"Converting response to CategoryDto...");
            var responseObject = await response.ResponseToDto<Category>(c => new CategoryDto(c));
            _logger.Trace($"Response successfully converted to CategoryDto");
            _logger.Debug($"Creating category method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] CategoryDto categoryDto)
        {
            _logger.Debug($"Start editing category by id={id} method...");
            _logger.Trace($"Editing category by id={id} in MS Pim...");
            var response = await _pimClient.Categories.Edit(id, categoryDto.ToEntity());
            _logger.Trace($"Category with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to CategoryDto...");
            var responseObject = await response.ResponseToDto<Category>(c => new CategoryDto(c));
            _logger.Trace($"Response successfully converted to CategoryDto");
            _logger.Debug($"Editing category by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(Category), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.Debug($"Start deleting category by id={id} method...");
            _logger.Trace($"Deleting category by id={id} in MS Pim...");
            var response = await _pimClient.Categories.Delete(id);
            _logger.Trace($"Category with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to CategoryDto...");
            var responseObject = await response.ResponseToDto<Category>(c => new CategoryDto(c));
            _logger.Trace($"Response successfully converted to CategoryDto");
            _logger.Debug($"Deleting category by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
   
        }
    }
}