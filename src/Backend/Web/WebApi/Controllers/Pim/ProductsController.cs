using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models;
using Company.Common.Models.Pim;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Extensions;

namespace WebApi.Controllers.Pim
{
    [Route("v1/[controller]")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IHttpPimClient _pimClient;
        private readonly Logger _logger;

        public ProductsController(IHttpPimClient pimClient)
        {
            _pimClient = pimClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] bool withParents = false)
        {
            _logger.Debug($"Start getting product by id={id} method...");
            _logger.Trace($"Getting product by id={id} from MS Pim...");
            var response = await _pimClient.Products.GetById(id, withParents);
            _logger.Trace($"Product by id={id} from MS Pim successfully received");
            _logger.Trace($"Converting response to ProductDto...");
            var responseObject = await response.ResponseToDto<Product>(p => new ProductDto(p));
            _logger.Trace($"Response successfully converted to ProductDto");
            _logger.Debug($"Getting product by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("ids")]
        [ProducesResponseType(typeof(List<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductCmpyIds()
        {
            var productsIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start getting products by ids={string.Join(",", productsIds)} method...");
            _logger.Trace($"Getting products by ids={string.Join(",", productsIds)} from MS Pim...");
            var response = await _pimClient.Products.GetProductCmpyIds(productsIds);
            _logger.Trace($"Products by ids={string.Join(",", productsIds)} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Getting products by ids={string.Join(",", productsIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
  
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromQuery] int pageSize, [FromQuery] int pageNumber, [FromQuery] bool withoutCategory, [FromQuery] string sku, [FromQuery] string name, [FromQuery] List<SearchAttributeDto> searchAttributes, [FromQuery] int? importId)
        {
            var parameters = $"pageSize={pageSize}, withoutCategory={withoutCategory}, sku={sku}, name={name}, importId={importId}";
            _logger.Debug($"Start getting products by {parameters} method...");
            _logger.Trace($"Getting products by {parameters} from MS Pim...");
            var response = await _pimClient.Products.GetByParams(pageSize, pageNumber, withoutCategory, sku, name, importId);
            _logger.Trace($"products by {parameters} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Getting products by {parameters} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("calculator")]
        [ProducesResponseType(typeof(List<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductCmpyBrandAndSeason([FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Debug($"Start getting products by brandId={brandId} and seasonId={seasonId} method...");
            _logger.Trace($"Getting products by brandId={brandId} and seasonId={seasonId} from MS Pim...");
            var response = await _pimClient.Products.GetByBrandAndSeason(seasonId, brandId);
            _logger.Trace($"Products by brandId={brandId} and seasonId={seasonId} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Getting products by brandId={brandId} and seasonId={seasonId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost("deals")]
        [ProducesResponseType(typeof(List<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetProductCmpySku([FromBody] List<string> skus)
        {
           
            _logger.Debug($"Start getting products by skus={string.Join(",", skus)} method...");
            _logger.Trace($"Getting products by skus={string.Join(",", skus)} from MS Pim...");
            var response = await _pimClient.Products.GetProductCmpySku(skus);
            _logger.Trace($"Products by skus={string.Join(",", skus)} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Getting products by skus={string.Join(",", skus)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost("ids")]
        public async Task<IActionResult> GetProductCmpyIdsPost([FromBody] int[] ids)
        {
            _logger.Debug($"Start getting products by ids={string.Join(",", ids)} method...");
            _logger.Trace($"Getting products by ids={string.Join(",", ids)} from MS Pim...");
            var response = await _pimClient.Products.GetProductCmpyIdsPost(ids.ToList());
            _logger.Trace($"Products by ids={string.Join(",", ids)} from MS Pim successfully received");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Getting products by ids={string.Join(",", ids)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }


        [HttpPut("update-search")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateSearchString()
        {
            _logger.Debug($"Start updating search string method...");
            _logger.Trace($"Updating search string in MS Pim...");
            var response = await _pimClient.Products.UpdateSearchString();
            _logger.Trace($"Search string successfully updated in MS Pim");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Updating search string method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("update-search")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateSearchStringArray()
        {
            _logger.Debug($"Start updating search array method...");
            _logger.Trace($"Updating search array in MS Pim...");
            var response = await _pimClient.Products.UpdateSearchStringArray();
            _logger.Trace($"Search array successfully updated in MS Pim");
            _logger.Trace($"Converting response to object...");
            var responseObject = await response.ResponseToDto<object>();
            _logger.Trace($"Response successfully converted to object");
            _logger.Debug($"Updating search array method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("search")]
        [ProducesResponseType(typeof(PagedResult<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> SearchProducts([FromQuery] int? pageSize, [FromQuery] int? pageNumber, [FromQuery] string sortByPropertyName, [FromQuery] bool withoutCategory, [FromQuery] bool sortByAsc, 
                                                        [FromQuery] string ageMonthRange = "-", [FromQuery] string ageYearRange = "-", [FromQuery] string priceRange = "-", [FromQuery] string currency = "EUR")
        {

            _logger.Debug($"Start search products method...");
            _logger.Trace($"Getting categories from query params...");
            var categories = Request.Query["categories"].Count == 0
                                       ? new List<int>()
                                       : Request.Query["categories"].ToString()
                                                                    .Split(',')
                                                                    .Select(int.Parse)
                                                                    .ToList();

            //var attributesIds = new List<int> { 138, 125, 177 };
            _logger.Trace($"Getting attributesIds from query params...");
            var attributesIds = Request.Query["attributesIds"].Count == 0
                           ? new List<int>()
                           : Request.Query["attributesIds"].ToString()
                                                        .Split(',')
                                                        .Select(int.Parse)
                                                        .ToList();

            _logger.Trace($"Getting searchString from query params...");
            var searchStr = Request.Query["searchString"].ToList();

            _logger.Trace($@"Getting products by pageSize={pageSize},pageNumber={pageNumber},withoutCategory={withoutCategory},categories={string.Join(",", categories)},attributesIds={string.Join(",", attributesIds)},
            searchStr={string.Join(",", searchStr)},sortByPropertyName={sortByPropertyName},sortByAsc={sortByAsc},ageMonthRange={ageMonthRange},ageYearRange={ageYearRange}, priceRange={priceRange}, currency={currency} from MS Pim...");
            var response = await _pimClient.Products.Search(pageSize, pageNumber, withoutCategory, categories, attributesIds, searchStr, sortByPropertyName, sortByAsc, ageMonthRange, ageYearRange, priceRange, currency);
            if (response.Headers.TryGetValues("X-Total-Count", out IEnumerable<string> values))
            {
                _logger.Trace($"Adding X-Total-Count header to response with value={values.First()} ...");
                Request.HttpContext.Response.Headers.Add("X-Total-Count", values.First());
            }
            _logger.Trace($"Adding Access-Control-Expose-Headers header to response...");
            Request.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");
            _logger.Trace($"Converting response to PagedResult of ProductDto...");
            var responseObject = await response.ResponseToDto<PagedResult<Product>>((pr) => new PagedResult<ProductDto>()
            {
                CurrentPage = pr.CurrentPage,
                PageCount = pr.PageCount,
                PageSize = pr.PageSize,
                RowCount = pr.RowCount,
                Results = pr.Results.Select(p => new ProductDto(p)).ToList(),
            });
            _logger.Trace($"Response successfully converted to PagedResult of ProductDto");
            _logger.Debug($"Search products  method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Create([FromBody] ProductDto productDto)
        {
            _logger.Debug($"Start creating product method...");
            _logger.Trace($"Creating product in MS Pim...");
            var response = await _pimClient.Products.Create(productDto.ToEntity());
            _logger.Trace($"Product successfully created in MS Pim");
            _logger.Trace($"Converting response to ListDto...");
            var responseObject = await response.ResponseToDto<Product>(p => new ProductDto(p));
            _logger.Trace($"Response successfully converted to ListDto");
            _logger.Debug($"Creating product method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("properties")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateAttributeValuesFromPropertiesDto([FromBody] List<ProductProperties> prodProperties)
        {
            _logger.Debug($"Start creating attribute values method...");
            _logger.Trace($"Creating  attribute values in MS Pim...");
            var response = await _pimClient.Products.CreateAttributeValues(prodProperties.Select(pp => pp.ToEntity()).ToList());
            _logger.Trace($"Attribute values successfully created in MS Pim");
            _logger.Trace($"Converting response to Object...");
            var responseObject = await response.ResponseToDto<Object>();
            _logger.Trace($"Response successfully converted to Object");
            _logger.Debug($"Creating attribute values method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] ProductDto productDto)
        {
            _logger.Debug($"Start editing product by id={id} method...");
            _logger.Trace($"Editing product by id={id} in MS Pim...");
            var response = await _pimClient.Products.Edit(id, productDto.ToEntity());
            _logger.Trace($"Product with id={id} successfully edited in MS Pim");
            _logger.Trace($"Converting response to ProductDto...");
            var responseObject = await response.ResponseToDto<Product>(p => new ProductDto(p));
            _logger.Trace($"Response successfully converted to ProductDto");
            _logger.Debug($"Editing product by id={id} method is finished");
           
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ProductDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.Debug($"Start deleting product by id={id} method...");
            _logger.Trace($"Deleting product by id={id} in MS Pim...");
            var response = await _pimClient.Products.DeleteById(id);
            _logger.Trace($"Product with id={id} successfully deleted in MS Pim");
            _logger.Trace($"Converting response to ProductDto...");
            var responseObject = await response.ResponseToDto<Product>(p => new ProductDto(p));
            _logger.Trace($"Response successfully converted to ProductDto");
            _logger.Debug($"Deleting product by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(List<ProductDto>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteProducts()
        {
            var productsIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start deleting products by ids={string.Join(",", productsIds)} method...");
            _logger.Trace($"Deleting products by ids={string.Join(",", productsIds)} from MS Pim...");
            var response = await _pimClient.Products.DeleteByIds(productsIds);
            _logger.Trace($"Products by ids={string.Join(",", productsIds)} successfully deleted in MS Pim ");
            _logger.Trace($"Converting response to List of ProductDto...");
            var responseObject = await response.ResponseToDto<List<Product>>((lp) => lp.Select(p => new ProductDto(p)));
            _logger.Trace($"Response successfully converted to List of ProductDto");
            _logger.Debug($"Deleting products by ids={string.Join(",", productsIds)} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}