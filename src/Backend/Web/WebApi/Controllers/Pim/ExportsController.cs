using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Company.Common.Models.Pim;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Dto.Pim;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using WebApi.Clients;
using WebApi.Extensions;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class ExportsController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Logger _logger;

        public ExportsController(IHttpPimClient pimClient, IHttpContextAccessor httpContextAccessor)
        {
            _pimClient = pimClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPost("gtin")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> ExportGtin([FromQuery] bool withoutCategory, [FromBody] List<int> productsIds)
        {
            _logger.Debug($"Start exporting gtin method...");
            _logger.Trace($"Exporting gtin from MS Pim...");
            var categoriesIds = Request.GetIdsFromQuery("categories");
            var response = await _pimClient.Exports.ExportGtin(productsIds, withoutCategory, categoriesIds, Request.Query["searchString"]); ;
            _logger.Trace($"Export file from MS Pim successfully received");
            _logger.Trace($"Converting response stream to file...");
            var responseFile = await response.ResponseToFile(_httpContextAccessor);
            _logger.Trace($"Response stream successfully converted to file");
            _logger.Debug($"Exporting gtin method is finished");
            return responseFile;
        }


        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateExportFile([FromQuery] int? templateCategoryId, [FromQuery] bool withoutCategory, [FromBody] List<int> productsIds)
        {
            _logger.Debug($"Start exporting products method...");
            _logger.Trace($"Exporting products from MS Pim...");
            var categoriesIds = Request.GetIdsFromQuery("categories");
            var response = await _pimClient.Exports.CreateExportFile(productsIds, templateCategoryId, withoutCategory, categoriesIds, Request.Query["searchString"]);
            _logger.Trace($"Export file from MS Pim successfully received");
            _logger.Trace($"Converting response stream to file...");
            var responseFile = await response.ResponseToFile(_httpContextAccessor);
            _logger.Trace($"Response stream successfully converted to file");
            _logger.Debug($"Exporting products method is finished");
            return responseFile;
        }
    }
}