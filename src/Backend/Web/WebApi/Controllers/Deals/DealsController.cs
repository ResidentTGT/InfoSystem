using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Deals;
using Company.Common.Models.Seasons;
using WebApi.Clients;
using WebApi.Dto.Deals;
using WebApi.Extensions;

namespace WebApi.Controllers.Deals
{
    [Route("v1/[controller]")]
    public class DealsController : Controller
    {
        private IHttpContextAccessor _httpContextAccessor;
        private IHttpDealsClient _httpDealsClient;
        private IdentityContext _identityContext;
        private readonly Logger _logger;

        public DealsController(IHttpContextAccessor httpContextAccessor, IHttpDealsClient httpDealsClient, IdentityContext identityContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpDealsClient = httpDealsClient;
            _identityContext = identityContext;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<DealDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAllDeals([FromQuery] int? pageSize, [FromQuery] int? pageNumber,
            [FromQuery] string contractor, [FromQuery] float? discountFrom, [FromQuery] float? discountTo,
            [FromQuery] int? dealId, [FromQuery] DateTime? loadDateFrom, [FromQuery] DateTime? loadDateTo,
            [FromQuery] DateTime? createDateFrom, [FromQuery] DateTime? createDateTo)
        {
            var parameters = $@"pageSize={pageSize}, pageNumber={pageNumber}, contractor={contractor}, discountFrom={discountFrom},discountTo={discountTo},
            dealId={dealId}, loadDateFrom={loadDateFrom}, loadDateTo={loadDateTo}, createDateFrom={createDateFrom}, createDateTo={createDateTo}";

            _logger.Debug($"Start getting deals by {parameters} method...");

            _logger.Trace($"Getting query parameters for brandsIds, seasonsIds, departmentsIds, managersIds...");
            var brandsIds = Request.GetIdsFromQuery("brands");
            var seasonsIds = Request.GetIdsFromQuery("seasons");
            var departmentsIds = Request.GetIdsFromQuery("departments");
            var managersIds = Request.GetIdsFromQuery("managers");
            _logger.Trace($"Query parameters for brandsIds, seasonsIds, departmentsIds, managersIds were successfully parsed");

            _logger.Trace($"Getting deals by {parameters} from MS Deals...");
            var response = await _httpDealsClient.Deals.GetAllDeals(pageSize, pageNumber, brandsIds, seasonsIds, departmentsIds, managersIds, contractor,
                discountFrom, discountTo, dealId, loadDateFrom, loadDateTo, createDateFrom, createDateTo);
            _logger.Trace($"Deals by {parameters} from MS Deals successfully received");
            _logger.Trace($"Converting response to List of DealDto...");
            var responseObject = await response.ResponseToDto<List<Deal>>(d => d.Select(deal => new DealDto(deal, GetManagerName(deal.ManagerId))));
            _logger.Trace($"Response successfully converted to List of DealDto");
            _logger.Debug($"Getting deals by  {parameters}  method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDealById([FromRoute] int id)
        {
            _logger.Debug($"Start getting deal by id={id} method...");
            _logger.Trace($"Getting deal by id={id} from MS Deals...");
            var response = await _httpDealsClient.Deals.GetDealById(id);
            _logger.Trace($"Deal by id={id} from MS Deals successfully received");

            _logger.Trace($"Getting X-DealProducts-Count value from headers...");
            int? count = null;
            if (response.Headers.TryGetValues("X-DealProducts-Count", out IEnumerable<string> values))
                count = Convert.ToInt32(values.First());
            _logger.Trace($"X-DealProducts-Count value successfully got");

            _logger.Trace($"Converting response to DealDto...");
            var responseObject = await response.ResponseToDto<Deal>(d => new DealDto(d, GetManagerName(d.ManagerId), count));
            _logger.Trace($"Response successfully converted to DealDto");
            _logger.Debug($"Getting deal by id={id} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpGet("files/{id}")]
        [ProducesResponseType(typeof(byte[]), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDownloadFile([FromRoute] int id)
        {
            _logger.Debug($"Start getting download file by id={id} method...");
            _logger.Trace($"Getting download file by id={id} from MS Deals...");
            var response = await _httpDealsClient.Deals.GetDownloadFile(id);
            _logger.Trace($"Download file by id={id} from MS Deals successfully received");
            _logger.Trace($"Converting response stream to file...");
            var responseFile = await response.ResponseToFile(_httpContextAccessor);
            _logger.Trace($"Response stream successfully converted to file");
            _logger.Debug($"Getting download file by id={id} method is finished");
            return responseFile;
        }

        [HttpGet("marginalities")]
        [ProducesResponseType(typeof(List<float>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetDealsManagerMarginalities([FromQuery] int dealId)
        {
            _logger.Debug($"Start getting deals manager marginalities by dealId={dealId} method...");
            _logger.Trace($"Getting deals manager marginalities by dealId={dealId} from MS Deals...");
            var response = await _httpDealsClient.Deals.GetDealsManagerMarginalities(dealId);
            _logger.Trace($"Deals manager marginalities by dealId={dealId} from MS Deals successfully received");
            _logger.Trace($"Converting response to List of float...");
            var responseObject = await response.ResponseToDto<List<float>>();
            _logger.Trace($"Response successfully converted to List of float");
            _logger.Debug($"Getting deals manager marginalities by dealId={dealId} method is finished");

            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DealDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> LoadOrderForm()
        {
            _logger.Debug($"Start loading order form method...");

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
                _logger.Trace($"Load order form to MS Deals...");
                var response = await _httpDealsClient.Deals.LoadOrderForm(file);
                _logger.Trace($"Order form response from MS Deals successfully received");
                _logger.Trace($"Converting response to DealDto...");
                var responseObject = await response.ResponseToDto<Deal>(d => new DealDto(d, GetManagerName(d.ManagerId)));
                _logger.Trace($"Response successfully converted to DealDto");
                _logger.Debug($"Loading order form method is finished");
                return StatusCode((int) response.StatusCode, responseObject);
            }
        }

        [HttpPost("{dealId}/contract")]
        [ProducesResponseType((int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> SaveDealDocuments([FromRoute] int dealId)
        {
            _logger.Debug($"Start saving deal documents by dealId={dealId} method...");

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
                _logger.Trace($"Saving deal documents by dealId={dealId} to MS Deals...");
                var response = await _httpDealsClient.Deals.SaveDealDocuments(dealId, file);
                _logger.Trace($"Deal documents successfully saved in MS Deals");

                var responseObject = await response.ResponseToDto<object>();

                _logger.Debug($"Saving deal documents by dealId={dealId} method is finished");
                return StatusCode((int) response.StatusCode, responseObject);
            }
        }

        [HttpPost("head-discount-request")]
        [ProducesResponseType(typeof(HeadDiscountRequestDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateHeadDiscountRequest([FromBody] HeadDiscountRequestDto headDiscountRequest)
        {
            _logger.Debug($"Start creating head discount request method...");
            _logger.Trace($"Creating head discount request in MS Deals...");
            var response = await _httpDealsClient.Deals.CreateHeadDiscountRequest(headDiscountRequest.ToEntity());
            _logger.Trace($"Head discount request successfully created in MS Deals");
            _logger.Trace($"Converting response to HeadDiscountRequestDto...");
            var responseObject = await response.ResponseToDto<HeadDiscountRequest>(h => new HeadDiscountRequestDto(h));
            _logger.Trace($"Response successfully converted to HeadDiscountRequestDto");
            _logger.Debug($"Creating head discount request method is finished");

            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DealDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] DealDto deal)
        {
            _logger.Debug($"Start editing deal by id={id} method...");
            _logger.Trace($"Editing deal by id={id} in MS Deals...");
            var response = await _httpDealsClient.Deals.EditDeal(id, deal.ToEntity());
            _logger.Trace($"Deal with id={id} successfully edited in MS Deals");
            _logger.Trace($"Converting response to DealDto...");
            var responseObject = await response.ResponseToDto<Deal>(d => new DealDto(d, GetManagerName(d.ManagerId)));
            _logger.Trace($"Response successfully converted to DealDto");
            _logger.Debug($"Editing deal by id={id} method is finished");

            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPut("head-discount-request/{id}")]
        [ProducesResponseType(typeof(List<HeadDiscountRequestDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> EditHeadDiscountRequest([FromRoute] int id, [FromBody] HeadDiscountRequestDto headDiscountRequest)
        {
            _logger.Debug($"Start editing head discount request by id={id} method...");
            _logger.Trace($"Editing head discount request by id={id} in MS Deals...");
            var response = await _httpDealsClient.Deals.EditHeadDiscountRequest(id, headDiscountRequest.ToEntity());
            _logger.Trace($"Head discount request with id={id}successfully edited in MS Deals");
            _logger.Trace($"Converting response to HeadDiscountRequestDto...");
            var responseObject = await response.ResponseToDto<HeadDiscountRequest>(h => new HeadDiscountRequestDto(h));
            _logger.Trace($"Response successfully converted to HeadDiscountRequestDto");
            _logger.Debug($"Editing head discount request by id={id} method is finished");

            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpDelete]
        [ProducesResponseType(typeof(List<DealDto>), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> DeleteDeals()
        {
            var dealsIds = Request.GetIdsFromQuery();
            _logger.Debug($"Start deleting deals by ids={string.Join(",", dealsIds)} method...");
            _logger.Trace($"deleting deals by ids={string.Join(",", dealsIds)} in MS Deals...");
            var response = await _httpDealsClient.Deals.DeleteDeals(dealsIds);
            _logger.Trace($"Deals with ids={string.Join(",", dealsIds)} successfully deleted in MS Deals");
            _logger.Trace($"Converting response to DealDto...");
            var responseObject = await response.ResponseToDto<List<Deal>>(dl => dl.Select(d => new DealDto(d, GetManagerName(d.ManagerId))));
            _logger.Trace($"Response successfully converted to DealDto");
            _logger.Debug($"Deleting deals by ids={string.Join(",", dealsIds)} method is finished");

            return StatusCode((int) response.StatusCode, responseObject);
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