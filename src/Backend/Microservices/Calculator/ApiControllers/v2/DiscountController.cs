using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Company.Common.Models.Deals;
using Company.Common.Models.Seasons;
using Company.Calculator.Clients.v2;
using NLog;
using System.Net.Http;

namespace Company.Calculator.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class DiscountController : Controller
    {
        private Calculation.Calculation _calculator { get; set; }
        private readonly Logger _logger;

        public DiscountController()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("marginalities")]
        public IActionResult GetCalcParams([FromQuery] int dealId)
        {
            _logger.Log(LogLevel.Debug, "Starting the get calc params method...");
            var dealsClient = new HttpDealsMSClient();
            var seasonsClient = new HttpSeasonsMSClient();

            _logger.Log(LogLevel.Trace, $"Getting deal by id='{dealId}'...");
            var deal = dealsClient.GetDeal(dealId, HttpContext);
            _logger.Log(LogLevel.Trace, $"Deal with id='{dealId}' successfully fetched");

            _logger.Log(LogLevel.Trace, $"Getting marginalities for deal with id='{dealId}'...");
            var listOfSeasonMarginalities = dealsClient.GetMarginalities(dealId, HttpContext);
            _logger.Log(LogLevel.Trace, "Marginalities successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting average of marginalities...");
            var averageMarginality = listOfSeasonMarginalities.DefaultIfEmpty(0).Average();
            _logger.Log(LogLevel.Trace, "Average of marginalities successfully fetched");

            _logger.Log(LogLevel.Trace, $"Getting policy for season='{deal.SeasonId}'...");
            var policy = seasonsClient.GetPolicyBySeasonValue(deal.SeasonId, HttpContext);
            _logger.Log(LogLevel.Trace, "Policy successfully fetched");

            _logger.Log(LogLevel.Debug, "Get calc params method is over.");
            return Ok(new CalcParams
            {
                SeasonMarginality = averageMarginality,
                CoefA = averageMarginality * listOfSeasonMarginalities.Count / (listOfSeasonMarginalities.Count + 1),
                CoefB = listOfSeasonMarginalities.Count + 1,
                CoefC = deal.NetCostPure / deal.VolumePure,
                MarginalityPlan = GetSalesPlanDependsOnPartnerType(policy.SalesPlanDatas.LastOrDefault(), deal.PartnersType),
                PrepaymentLimit = policy.BrandPolicyDatas.First(b => b.BrandName.Trim().ToUpper() == deal.Brand.Trim().ToUpper()).PrepaymentVolumePercent
            });
        }

        [HttpGet]
        public IActionResult GetMaxDiscounts([FromQuery] int dealId, [FromQuery] Common.Enums.ContractType contractType, [FromQuery] Common.Enums.OrderType orderType, [FromQuery] float headDiscount, [FromQuery] float ceoDiscount, [FromQuery] float installment, [FromQuery] float prepayment, [FromQuery] bool considerMarginality)
        {
            _logger.Log(LogLevel.Debug, "Starting the get max discounts method...");

            _logger.Log(LogLevel.Trace, "Creating discount params...");
            var discountParams = new DiscountParams()
            {
                DealId = dealId,
                ContractType = contractType,
                OrderType = orderType,
                Installment = installment,
                Prepayment = prepayment,
                ConsiderMarginality = considerMarginality,
                CeoDiscount = ceoDiscount,
                HeadDiscount = headDiscount
            };
            _logger.Log(LogLevel.Trace, "Creating discount params is finished");

            var client = new HttpDealsMSClient();
            var dealsClient = new HttpDealsMSClient();
            var seasonsClient = new HttpSeasonsMSClient();

            _logger.Log(LogLevel.Trace, $"Getting deal by id='{dealId}'...");
            var deal = client.GetDeal(discountParams.DealId, HttpContext);
            _logger.Log(LogLevel.Trace, "Deal successfully fetched");

            _logger.Log(LogLevel.Trace, $"Getting marginalities for deal with id='{dealId}'...");
            var listOfSeasonMarginalities = dealsClient.GetMarginalities(dealId, HttpContext);
            _logger.Log(LogLevel.Trace, "Marginalities successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting average of marginalities...");
            var averageMarginality = listOfSeasonMarginalities.Select(m => m).DefaultIfEmpty(0).Average();
            _logger.Log(LogLevel.Trace, "Average of marginalities successfully fetched");

            deal.DiscountParams.Add(discountParams);

            _logger.Log(LogLevel.Trace, $"Getting policy for season='{deal.SeasonId}'...");
            var policy = seasonsClient.GetPolicyBySeasonValue(deal.SeasonId, HttpContext);
            _logger.Log(LogLevel.Trace, "Policy successfully fetched");

            _logger.Log(LogLevel.Trace, "Creating calculator...");
            _calculator = new Calculation.Calculation(policy);
            _logger.Log(LogLevel.Trace, "Creating calculator is finished");

            _logger.Log(LogLevel.Debug, "Get max discounts method is over.");
            return Ok(new MaxDiscounts
            {
                MaxDiscount = _calculator.CalculateMaxDiscount(deal, headDiscount, ceoDiscount, listOfSeasonMarginalities.Count, averageMarginality),
                MaxManagerDiscount = _calculator.CalculateMaxDiscount(deal, 0, 0, listOfSeasonMarginalities.Count, averageMarginality),
                DealId = discountParams.DealId
            });
        }

        [HttpPut("deals/{id}")]
        public async Task<IActionResult> SaveDeal([FromRoute] int id, [FromBody] Deal deal)
        {
            _logger.Log(LogLevel.Debug, $"Starting the save deal with id='{deal.Id}' method...");
            var dealsClient = new HttpDealsMSClient();
            var seasonsClient = new HttpSeasonsMSClient();

            _logger.Log(LogLevel.Trace, $"Getting marginalities for deal with id='{deal.Id}'...");
            var listOfSeasonMarginalities = dealsClient.GetMarginalities(deal.Id, HttpContext);
            _logger.Log(LogLevel.Trace, "Marginalities successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting average of marginalities...");
            var averageMarginality = listOfSeasonMarginalities.Select(m => m).DefaultIfEmpty(0).Average();
            _logger.Log(LogLevel.Trace, "Average of marginalities successfully fetched");

            _logger.Log(LogLevel.Trace, $"Getting policy for season='{deal.SeasonId}'...");
            var policy = seasonsClient.GetPolicyBySeasonValue(deal.SeasonId, HttpContext);

            if (policy == null)
            {
                _logger.Log(LogLevel.Error, $"Policy with season='{deal.SeasonId}' not found");
                return BadRequest("Отсутствует политика");
            }
            _logger.Log(LogLevel.Trace, "Policy with season='{deal.SeasonId}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Creating calculator...");
            _calculator = new Calculation.Calculation(policy);
            _logger.Log(LogLevel.Trace, "Creating calculator is finished");

            _logger.Log(LogLevel.Trace, "Calculation max discount...");
            var maxManagerDiscount = _calculator.CalculateMaxDiscount(deal, 0, 0, listOfSeasonMarginalities.Count, averageMarginality);
            _logger.Log(LogLevel.Trace, $"Calculation max discount='{maxManagerDiscount}' is finished");

            _logger.Log(LogLevel.Trace, "Getting discount...");
            var discountM = Math.Min(deal.Discount, maxManagerDiscount);
            _logger.Log(LogLevel.Trace, $"Discount='{discountM}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Calculation manager marginality...");
            deal.ManagerMarginality = (1 - (deal.NetCostPure / (deal.VolumePure * (1 - discountM / 100)))) * 100;
            _logger.Log(LogLevel.Trace, $"Calculation manager marginality='{deal.ManagerMarginality}' is finished");

            _logger.Log(LogLevel.Trace, $"Putting deal with id='{deal.Id}'...");
            var response = await dealsClient.PutDealAsync(deal, HttpContext);

            if (response.IsSuccessStatusCode)
                _logger.Log(LogLevel.Trace, $"Putting deal with id='{deal.Id}' was successfully finished");
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();

                _logger.Log(LogLevel.Error, $"Error in putting deal with id='{deal.Id}'. Error: { errorMessage}");
                return BadRequest(errorMessage);
            }


            _logger.Log(LogLevel.Debug, $"Save deal with id='{deal.Id}' method is over.");
            return Ok(deal);
        }

        private float GetSalesPlanDependsOnPartnerType(SalesPlanData salesPlanDataDto, Common.Enums.PartnersType partnersType)
            => (partnersType == Common.Enums.PartnersType.Internet || partnersType == Common.Enums.PartnersType.Wholesale)
                ? salesPlanDataDto.WholesaleMarginality
                : salesPlanDataDto.NetworkMarginality;

        internal class MaxDiscounts
        {
            public int DealId { get; set; }
            public float MaxDiscount { get; set; }
            public float MaxManagerDiscount { get; set; }
        }

        internal class CalcParams
        {
            public float MarginalityPlan { get; set; }
            public float SeasonMarginality { get; set; }
            public float CoefA { get; set; }
            public float CoefB { get; set; }
            public float CoefC { get; set; }
            public float PrepaymentLimit { get; set; }
        }
    }
}