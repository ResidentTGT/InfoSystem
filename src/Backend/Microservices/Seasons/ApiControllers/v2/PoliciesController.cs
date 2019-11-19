using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Company.Common.Extensions;
using Company.Common.Models.Seasons;
using NLog;

namespace Company.Seasons.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class PoliciesController : Controller
    {
        private readonly SeasonsContext _context;
        private readonly Logger _logger;

        public PoliciesController(SeasonsContext context)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();
        }


        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int seasonId)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get policy by season id='{seasonId}' method...");

            _logger.Log(LogLevel.Trace, $"Getting policy with seasonId='{seasonId}'");
            var policy = await _context.DiscountPolicies.Include(dp => dp.SalesPlanDatas)
                .Include(dp => dp.PolicyDatas)
                .Include(dp => dp.BrandPolicyDatas)
                .Include(dp => dp.ExchangeRates)
                .FirstOrDefaultAsync(l => l.SeasonListValueId == seasonId);

            if (policy == null)
            {
                _logger.Log(LogLevel.Error, $"Policy with seasonId='{seasonId}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Policy with seasonId='{seasonId}' successfully fetched");

            policy.SalesPlanDatas = policy.SalesPlanDatas.Where(spd => spd.DeleteTime == null).ToList();
            policy.PolicyDatas = policy.PolicyDatas.Where(spd => spd.DeleteTime == null).ToList();
            policy.ExchangeRates = policy.ExchangeRates.Where(spd => spd.DeleteTime == null).ToList();
            policy.BrandPolicyDatas = policy.BrandPolicyDatas.Where(spd => spd.DeleteTime == null).ToList();

            _logger.Log(LogLevel.Debug, $"Get policy by season id='{seasonId}' method is over.");
            return Ok(policy);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DiscountPolicy discountPolicy)
        {
            _logger.Log(LogLevel.Debug, "Starting the create policy method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }
            _logger.Log(LogLevel.Trace, $"Getting policy by season list value Id='{discountPolicy.SeasonListValueId}'...");
            var policy = await _context.DiscountPolicies.FirstOrDefaultAsync(dp => dp.SeasonListValueId == discountPolicy.SeasonListValueId);

            if (policy == null)
            {
                _logger.Log(LogLevel.Trace, $"Policy with season list value Id='{discountPolicy.SeasonListValueId}' not found...");
                var userId = Convert.ToInt32(Company.Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

                _logger.Log(LogLevel.Trace, "Updating fields of discount policy...");

                var currentTime = DateTime.Now;
                discountPolicy.CreateTime = currentTime;
                discountPolicy.CreatorId = userId;

                discountPolicy.BrandPolicyDatas.ForEach(bpd =>
                {
                    bpd.CreateTime = currentTime;
                    bpd.CreatorId = userId;
                });

                discountPolicy.PolicyDatas.ForEach(pd =>
                {
                    pd.CreateTime = currentTime;
                    pd.CreatorId = userId;
                });

                discountPolicy.SalesPlanDatas.ForEach(pd =>
                {
                    pd.CreateTime = currentTime;
                    pd.CreatorId = userId;
                });

                discountPolicy.ExchangeRates.ForEach(er =>
                {
                    er.CreateTime = currentTime;
                    er.CreatorId = userId;
                });
                _logger.Log(LogLevel.Trace, "Updating fields of discount policy is finished");
                _context.DiscountPolicies.Add(discountPolicy);

                _logger.Log(LogLevel.Trace, $"Saving policy with season list value Id='{discountPolicy.SeasonListValueId}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving policy. Error: {e.Message}");
                    throw new Exception("Error on save policy");
                }

                _logger.Log(LogLevel.Trace, $"Policy with season list value Id='{discountPolicy.SeasonListValueId}' were successfully saved");

                _logger.Log(LogLevel.Debug, "Create policy method is over.");
                return Ok(discountPolicy);
            }

            _logger.Log(LogLevel.Error, $"The policy with Id='{policy.Id}' for season with Id='{policy.SeasonListValueId}' already exists.");
            return BadRequest($"Политика для данного сезона уже существует.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] DiscountPolicy discountPolicy)
        {
            _logger.Log(LogLevel.Debug, $"Starting the edit policy by id='{id}' method...");
            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }
            _logger.Log(LogLevel.Trace, $"Searching policy by id='{id}'...");
            var discPolicyEntity = await _context.DiscountPolicies
                    .Include(dpe => dpe.ExchangeRates)
                    .Include(dpe => dpe.BrandPolicyDatas)
                    .Include(dpe => dpe.PolicyDatas)
                    .Include(dpe => dpe.SalesPlanDatas)
                    .FirstOrDefaultAsync(dp => dp.Id == id);
            if (discPolicyEntity == null)
            {
                _logger.Log(LogLevel.Info, $"Policy with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Policy with id='{id}' is found");

            var userId = Convert.ToInt32(Company.Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            var currentTime = DateTime.Now;

            var newSalesPlanData = discountPolicy.SalesPlanDatas.First();
            var oldSalesPlanData = discPolicyEntity.SalesPlanDatas.Where(spd => spd.DeleteTime == null).OrderBy(spd => spd.CreateTime).LastOrDefault();

            if (oldSalesPlanData == null || !AreEqualSalesPlanDatas(newSalesPlanData, oldSalesPlanData))
            {
                if (oldSalesPlanData != null)
                {
                    oldSalesPlanData.DeleteTime = currentTime;
                    oldSalesPlanData.DeleterId = userId;
                }

                discPolicyEntity.SalesPlanDatas.Add(new SalesPlanData
                {
                    CreateTime = currentTime,
                    CreatorId = userId,
                    NetworkMarginality = newSalesPlanData.NetworkMarginality,
                    WholesaleMarginality = newSalesPlanData.WholesaleMarginality
                });
            }

            var newBrandPolicyDatas = discountPolicy.BrandPolicyDatas;
            var oldBrandPolicyDatas = discPolicyEntity.BrandPolicyDatas
                 .Where(bpd => bpd.DeleteTime == null && newBrandPolicyDatas.Select(b => b.BrandName).Contains(bpd.BrandName))
                 .GroupBy(bpd => bpd.BrandName)
                 .Select(bpd => bpd.OrderByDescending(pd => pd.CreateTime).FirstOrDefault());

            newBrandPolicyDatas.ForEach(newBrandPolicyData =>
            {
                var oldBrandPolicyData = oldBrandPolicyDatas.FirstOrDefault(bpd => bpd.BrandName == newBrandPolicyData.BrandName);
                if (oldBrandPolicyData == null || !AreEqualBrandPolicyDatas(newBrandPolicyData, oldBrandPolicyData))
                {
                    if (oldBrandPolicyData != null)
                    {
                        oldBrandPolicyData.DeleteTime = currentTime;
                        oldBrandPolicyData.DeleterId = userId;
                    }

                    newBrandPolicyData.CreateTime = currentTime;
                    newBrandPolicyData.CreatorId = userId;
                    newBrandPolicyData.Id = 0;
                    discPolicyEntity.BrandPolicyDatas.Add(newBrandPolicyData);
                }
            });

            var newPolicyData = discountPolicy.PolicyDatas.First();
            var oldPolicyData = discPolicyEntity.PolicyDatas.Where(spd => spd.DeleteTime == null).OrderBy(spd => spd.CreateTime).LastOrDefault();
            if (oldPolicyData == null || !AreEqualPolicyDatas(newPolicyData, oldPolicyData))
            {
                if (oldPolicyData != null)
                {
                    oldPolicyData.DeleteTime = currentTime;
                    oldPolicyData.DeleterId = userId;
                }

                newPolicyData.CreateTime = currentTime;
                newPolicyData.CreatorId = userId;
                newPolicyData.Id = 0;
                discPolicyEntity.PolicyDatas.Add(newPolicyData);
            }


            var newExchangeRates = discountPolicy.ExchangeRates.First();
            var oldExchangeRates = discPolicyEntity.ExchangeRates.Where(spd => spd.DeleteTime == null).OrderBy(spd => spd.CreateTime).LastOrDefault();
            if (oldExchangeRates == null || !AreEqualExchangeRates(newExchangeRates, oldExchangeRates))
            {
                if (oldExchangeRates != null)
                {
                    oldExchangeRates.DeleteTime = currentTime;
                    oldExchangeRates.DeleterId = userId;
                }

                newExchangeRates.CreateTime = currentTime;
                newExchangeRates.CreatorId = userId;
                newExchangeRates.Id = 0;
                discPolicyEntity.ExchangeRates.Add(newExchangeRates);
            }

            _logger.Log(LogLevel.Trace, $"Saving changes in policy with id='{id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving policy with id='{id}'. Error: {e.Message}");
                throw new Exception($"Error on save policy with id='{id}'");
            }

            _logger.Log(LogLevel.Trace, $"Policy with id='{id}' were successfully saved");

            _logger.Log(LogLevel.Debug, $"Edit policy with id='{id}' method is over.");
            return Ok(discountPolicy);
        }

        [HttpGet("exchange-rates")]
        public async Task<IActionResult> GetExchangeRates([FromQuery(Name = "seasonId")] int seasonListValueId)
        {
            _logger.Log(LogLevel.Debug, $"Start getting exchange rates by seasonListValueId={seasonListValueId}...");

            _logger.Log(LogLevel.Trace, $"Getting exchange rates by seasonListValueId={seasonListValueId}...");
            var policy = await _context.DiscountPolicies
                                               .Include(dp => dp.ExchangeRates)
                                               .SingleOrDefaultAsync(dp => dp.SeasonListValueId == seasonListValueId);
            if (policy == null)
            {
                _logger.Log(LogLevel.Error, $"Policy by seasonListValueId={seasonListValueId} is not found");
                return NotFound($"Policy by seasonListValueId={seasonListValueId} is not found");
            }

            var exchangeRates = policy.ExchangeRates.OrderBy(er => er.CreateTime).LastOrDefault();

            if (exchangeRates == null)
            {
                _logger.Log(LogLevel.Error, $"Exchange rates in policy by seasonListValueId={seasonListValueId} is not found");
                return NotFound($"Exchange rates in policy by seasonListValueId={seasonListValueId} is not found");
            }
            _logger.Log(LogLevel.Trace, $"Exchange rates by seasonListValueId={seasonListValueId} is successfully received");

            _logger.Log(LogLevel.Debug, $"Getting exchange rates by seasonListValueId={seasonListValueId} is finished.");
            return Ok(exchangeRates);
        }

        private bool AreEqualSalesPlanDatas(SalesPlanData s1, SalesPlanData s2) =>
                s1.WholesaleMarginality == s2.WholesaleMarginality && s1.NetworkMarginality == s2.NetworkMarginality;

        private bool AreEqualBrandPolicyDatas(BrandPolicyData bpd1, BrandPolicyData bpd2) =>
               bpd1.BrandName == bpd2.BrandName
               && bpd1.CutoffDiscountPercent == bpd2.CutoffDiscountPercent
               && bpd1.PrepaymentVolumePercent == bpd2.PrepaymentVolumePercent
               && bpd1.Volume == bpd2.Volume;

        private bool AreEqualPolicyDatas(PolicyData pd1, PolicyData pd2) =>
               pd1.AnnualRate == pd2.AnnualRate
               && pd1.BrandMixDiscount == pd2.BrandMixDiscount
               && pd1.Commission == pd2.Commission
               && pd1.FreeWarehouseCurrentOrderDiscount == pd2.FreeWarehouseCurrentOrderDiscount
               && pd1.FreeWarehousePastOrderDiscount == pd2.FreeWarehousePastOrderDiscount
               && pd1.InternetKeyPartnerImportanceDiscount == pd2.InternetKeyPartnerImportanceDiscount
               && pd1.InternetPartnerImportanceDiscount == pd2.InternetPartnerImportanceDiscount
               && pd1.KeyPartnerImportanceDiscount == pd2.KeyPartnerImportanceDiscount
               && pd1.MarkupForMismatchOfVolume == pd2.MarkupForMismatchOfVolume
               && pd1.MaxCountOfInstallmentPeriods == pd2.MaxCountOfInstallmentPeriods
               && pd1.NetworkPartnerImportanceDiscount == pd2.NetworkPartnerImportanceDiscount
               && pd1.NewPartnerDiscount == pd2.NewPartnerDiscount
               && pd1.PlannedInstallment == pd2.PlannedInstallment
               && pd1.PlannedPrepayment == pd2.PlannedPrepayment
               && pd1.PreOrderDiscount == pd2.PreOrderDiscount
               && pd1.PrepaymentDiscount == pd2.PrepaymentDiscount
               && pd1.PurchaseAndSaleDiscount == pd2.PurchaseAndSaleDiscount
               && pd1.RepeatedPartnerDiscount == pd2.RepeatedPartnerDiscount
               && pd1.VolumeDiscount == pd2.VolumeDiscount
               && pd1.WholesalePartnerImportanceDiscount == pd2.WholesalePartnerImportanceDiscount;

        private bool AreEqualExchangeRates(ExchangeRates er1, ExchangeRates er2) =>
               er1.EurRub == er2.EurRub && er1.EurUsd == er2.EurUsd;

    }
}