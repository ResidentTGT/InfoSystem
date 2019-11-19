using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Enums;
using Company.Common.Models.Deals;
using WebApi.Clients;
using WebApi.Dto.Calculator;
using WebApi.Extensions;

namespace WebApi.Controllers.Calculator
{
    [Route("v1/[controller]")]
    public class DiscountController : Controller
    {
        private IHttpCalculatorClient _httpCalculatorClient;
        private readonly Logger _logger;

        public DiscountController(IHttpCalculatorClient httpCalculatorClient)
        {
            _httpCalculatorClient = httpCalculatorClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("marginalities")]
        [ProducesResponseType(typeof(CalcParamsDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetCalcParams([FromQuery] int dealId)
        {
            _logger.Debug($"Start getting calc params by dealId={dealId} method...");
            _logger.Trace($"Getting calc params by dealId={dealId} from MS Calculator...");
            var response = await _httpCalculatorClient.Discount.GetCalcParams(dealId);
            _logger.Trace($"Calc params by dealId={dealId} from MS Calculator successfully received");
            _logger.Trace($"Converting response to CalcParamsDto...");
            var responseObject = await response.ResponseToDto<CalcParamsDto>();
            _logger.Trace($"Response successfully converted to CalcParamsDto");
            _logger.Debug($"Getting calc params by dealId={dealId} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(MaxDiscountsDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetMaxDiscounts([FromQuery] int dealId, [FromQuery] ContractType contractType, [FromQuery] OrderType orderType, [FromQuery] float headDiscount, [FromQuery] float ceoDiscount, [FromQuery] float installment, [FromQuery] float prepayment, [FromQuery] bool considerMarginality)
        {
            var parameters = $"dealId={dealId},contractType={contractType},orderType={orderType},headDiscount={headDiscount},ceoDiscount={ceoDiscount},installment={installment},prepayment={prepayment},considerMarginality={considerMarginality}";
            _logger.Debug($"Start getting max discounts by {parameters} method...");
            _logger.Trace($"Getting max discounts by {parameters} from MS Calculator...");
            var response = await _httpCalculatorClient.Discount.GetMaxDiscounts(dealId, contractType, orderType, headDiscount, ceoDiscount, installment, prepayment, considerMarginality);
            _logger.Trace($"Max discounts by dealId={dealId} from MS Calculator successfully received");
            _logger.Trace($"Converting response to MaxDiscountsDto...");
            var responseObject = await response.ResponseToDto<MaxDiscountsDto>();
            _logger.Trace($"Response successfully converted to MaxDiscountsDto");
            _logger.Debug($"Getting max discounts by {parameters} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPut("deals/{id}")]
        [ProducesResponseType(typeof(DealDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.NotFound)]
        public async Task<IActionResult> SaveDeal([FromRoute] int id, [FromBody] DealDto deal)
        {
            _logger.Debug($"Start saving deal by id={id} method...");
            _logger.Trace($"Saving deal by id={id} from MS Calculator...");
            var response = await _httpCalculatorClient.Discount.SaveDeal(id, deal.ToEntity());
            _logger.Trace($"Deal by id={id} successfully saved in MS Calculator");
            _logger.Trace($"Converting response to DealDto...");
            var responseObject = await response.ResponseToDto<Deal>(d => new DealDto(d));
            _logger.Trace($"Response successfully converted to DealDto");
            _logger.Debug($"Saving deal by id={id} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }
    }
}