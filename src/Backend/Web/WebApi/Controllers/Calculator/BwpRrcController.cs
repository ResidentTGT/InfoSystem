using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Enums;
using Company.Common.Models.Pim;
using WebApi.Clients;
using WebApi.Dto.Calculator;
using WebApi.Extensions;

namespace WebApi.Controllers.Calculator
{
    [Route("v1/bwp-rrc")]
    public class BwpRrcController : Controller
    {
        private IHttpCalculatorClient _httpCalculatorClient;
        private readonly Logger _logger;

        public BwpRrcController(IHttpCalculatorClient httpCalculatorClient)
        {
            _httpCalculatorClient = httpCalculatorClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPut("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditSingleBwpAndRrc([FromRoute] int id, [FromBody] ProductDto product)
        {
            _logger.Debug($"Start editing BWP and RRC by productId={id} method...");
            _logger.Trace($"Editing BWP and RRC by productId={id} in MS Calculator...");
            var response = await _httpCalculatorClient.BwpRrc.EditSingleBwpAndRrc(id, product.ToEntity());
            _logger.Trace($"BWP and RRC by productId={id} successfully edited in MS Calculator");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Editing BWP and RRC by productId={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("properties")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditMultipleBwpAndRrc([FromQuery] RetailAttribute pin, [FromQuery] RetailAttribute editAttr, [FromBody] List<AttributeValueDto> attributeValues)
        {
            _logger.Debug($"Start mass editing BWP and RRC by pin={pin} editAttr={editAttr} method...");
            _logger.Trace($"Mass editing BWP and RRC by pin={pin} editAttr={editAttr} in MS Calculator...");
            var response = await _httpCalculatorClient.BwpRrc.EditMultipleBwpAndRrc(pin, editAttr, attributeValues.Select(av => av.ToEntity()).ToList());
            _logger.Trace($"BWP and RRC by pin={pin} editAttr={editAttr} successfully edited in MS Calculator");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Mass editing BWP and RRC by pin={pin} editAttr={editAttr} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);


        }
    }
}