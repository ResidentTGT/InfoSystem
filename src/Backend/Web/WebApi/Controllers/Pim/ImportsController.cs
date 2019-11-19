using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Pim;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Extensions;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class ImportsController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IdentityContext _identityContext;
        private readonly Logger _logger;

        public ImportsController(IHttpPimClient pimClient, IHttpContextAccessor httpContextAccessor, IdentityContext identityContext)
        {
            _pimClient = pimClient;
            _httpContextAccessor = httpContextAccessor;
            _identityContext = identityContext;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}/error")]
        [ProducesResponseType(typeof(ImportDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            _logger.Debug($"Start getting import error by id={id} method...");
            _logger.Trace($"Getting import error by id={id} from MS Pim...");
            var response = await _pimClient.Imports.Get(id);
            _logger.Trace($"Import error by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to string...");
            var responseObject = await response.Content.ReadAsStringAsync();
            _logger.Trace($"Response successfully converted to string");
            _logger.Debug($"Getting import error by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ImportDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get()
        {
            _logger.Debug($"Start getting imports method...");
            _logger.Trace($"Getting imports from MS Pim...");
            var response = await _pimClient.Imports.Get();
            _logger.Trace($"Imports from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ImportDto...");
            var responseObject = await response.ResponseToDto<List<Import>>(li => li.Select(i => new ImportDto(i, GetManagerName(i.CreatorId))));
            _logger.Trace($"Response successfully converted to List of ImportDto");
            _logger.Debug($"Getting imports method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Create()
        {
            _logger.Debug($"Starting import method...");
            _logger.Debug($"Getting file from request...");
            var file = _httpContextAccessor.HttpContext.Request.Form.Files.FirstOrDefault().FileToMultipartFormDataContent();
            if (file == null)
            {
                _logger.Error($"File not found in request");
                return NotFound();
            }

            _logger.Debug($"File successfully got");
            using (file)
            {
                _logger.Trace($"Import to MS Pim...");
                var response = await _pimClient.Imports.Create(file, Request.GetIdsFromQuery("necessaryAttributes"));
                _logger.Trace($"Import response from MS Pim successfully received");
                _logger.Trace($"Converting response to object...");
                var responseObject = await response.ResponseToDto<object>();
                _logger.Trace($"Response successfully converted to object");
                _logger.Debug($"Import method is finished");
                return StatusCode((int)response.StatusCode, responseObject);
            }
        }
        [HttpPost("old")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateOldImport()
        {
            _logger.Debug($"Starting old import method...");
            _logger.Debug($"Getting file from request...");
            var file = _httpContextAccessor.HttpContext.Request.Form.Files.FirstOrDefault().FileToMultipartFormDataContent();
            if (file == null)
            {
                _logger.Error($"File not found in request");
                return NotFound();
            }
            using (file)
            {
                _logger.Trace($"Old import to MS Pim...");
                var response = await _pimClient.Imports.CreateOldImport(file);
                _logger.Trace($"Old import response from MS Pim successfully received");
                _logger.Trace($"Converting response to object...");
                var responseObject = await response.ResponseToDto<object>();
                _logger.Trace($"Response successfully converted to object");
                _logger.Debug($"Old import method is finished");
                return StatusCode((int)response.StatusCode, responseObject);
            }
        }

        [HttpPost("gtin")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ImportGtin()
        {

            _logger.Debug($"Starting import gtin method...");
            _logger.Debug($"Getting file from request...");
            var file = _httpContextAccessor.HttpContext.Request.Form.Files.FirstOrDefault().FileToMultipartFormDataContent();
            if (file == null)
            {
                _logger.Error($"File not found in request");
                return NotFound();
            }

            _logger.Debug($"File successfully got");
            using (file)
            {
                _logger.Trace($"Import gtin to MS Pim...");
                var response = await _pimClient.Imports.ImportGtin(file);
                _logger.Trace($"Import gtin response from MS Pim successfully received");
                _logger.Trace($"Converting response to object...");
                var responseObject = await response.ResponseToDto<object>();
                _logger.Trace($"Response successfully converted to object");
                _logger.Debug($"Import gtin method is finished");
                return StatusCode((int)response.StatusCode, responseObject);
            }

           
        }

        private string GetManagerName(int managerId)
        {
            var user = _identityContext.Users.Find(managerId);

            if (user == null)
                return null;
            else
                return user.DisplayName != null ? user.DisplayName : user.UserName;
        }
    }
}