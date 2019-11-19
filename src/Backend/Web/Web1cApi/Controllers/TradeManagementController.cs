using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.Common.Models.Pim;
using Company.Common.Requests.Pim;
using Web1cApi.Communicators;
using Web1cApi.Dto;
using Web1cApi.Extensions;


namespace Web1cApi.Controllers
{
    [Route("v1/[controller]")]
    [Produces("application/xml")]
    public class TradeManagementController : Controller
    {
        private readonly IPimCommunicator _pimCommunicator;

        public TradeManagementController(IPimCommunicator pimCommunicator)
        {
            _pimCommunicator = pimCommunicator;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductXml([FromRoute] int id) => Ok(await _pimCommunicator.GetProduct(id));

        [HttpGet]
        [ProducesResponseType(typeof(List<Product1cDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductCmpySkuWithAttributesIds() => Ok(await _pimCommunicator.GetProductCmpySkuWithAttributesIds(Request.GetIdsFromQuery(paramName: "attributesIds"), Request.GetSkuFromQuery(paramName: "skus")));


        [HttpGet("sku")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductSkuCmpyGUID([FromQuery] string guidN, [FromQuery] string guidX) => Ok(await _pimCommunicator.GetProductSkuCmpyGUID(guidN, guidX));

        [HttpPost]
        [ProducesResponseType(typeof(List<Product1cDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductCmpySkuWithAttributesIdsPost([FromBody] ProductCmpySkusAndAttributesIdsRequest request) => Ok(await _pimCommunicator.GetProductCmpySkuWithAttributesIdsPost(request));
    }
}