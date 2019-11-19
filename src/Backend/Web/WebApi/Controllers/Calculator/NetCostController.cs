using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Pim;
using WebApi.Clients;
using WebApi.Dto.Calculator;
using WebApi.Extensions;

namespace WebApi.Controllers.Calculator
{
    [Route("v1/[controller]")]
    public class NetCostController : Controller
    {
        private IHttpCalculatorClient _httpCalculatorClient;
        private readonly Logger _logger;

        public NetCostController(IHttpCalculatorClient httpCalculatorClient)
        {
            _httpCalculatorClient = httpCalculatorClient;
            _logger = LogManager.GetCurrentClassLogger();
        }


        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditNetCost([FromRoute] int id, [FromQuery] bool recalculate, [FromBody] Product product, [FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Debug($"Start editing net cost by productId={id}, recalculate={recalculate}, brandId={brandId}, seasonId={seasonId}...");
            _logger.Trace($"Editing net cost by productId={id} in MS Calculator method...");
            var response = await _httpCalculatorClient.NetCost.EditNetCost(id, recalculate, product, brandId, seasonId);
            _logger.Trace($"Net cost by productId={id} successfully edited in MS Calculator");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Editing net cost by productId={id}, recalculate={recalculate}, brandId={brandId}, seasonId={seasonId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

        }

        [HttpPut("properties")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditMultipleNetCost([FromQuery] bool recalculate, [FromBody] List<ProductProperties> attributeValues, [FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Debug($"Start mass editing net cost by recalculate={recalculate}, brandId={brandId}, seasonId={seasonId} method...");
            _logger.Trace($"Mass editing net cost by recalculate={recalculate}, brandId={brandId}, seasonId={seasonId} in MS Calculator...");
            var response = await _httpCalculatorClient.NetCost.EditMultipleNetCost(recalculate, attributeValues.Select(x => x.AttributeValue.ToEntity()).ToList(), brandId, seasonId);
            _logger.Trace($"Net cost by recalculate={recalculate}, brandId={brandId}, seasonId={seasonId} successfully edited in MS Calculator");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Mass editing net cost by recalculate={recalculate}, brandId={brandId}, seasonId={seasonId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}