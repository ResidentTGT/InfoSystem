using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NLog;
using Company.Common.Enums;
using Company.Common.Models.Deals;
using Company.Common.Models.Seasons;

namespace Company.Calculator.Calculation
{
    public class Calculation
    {
        private readonly DiscountPolicy _policy;

        private readonly Logger _logger;

        public Calculation(DiscountPolicy policy)
        {
            _policy = policy;
            _policy.BrandPolicyDatas = _policy.BrandPolicyDatas.Where(bpd => bpd.DeleteTime == null).ToList();
            _logger = LogManager.GetCurrentClassLogger();
        }

        public float CalculateMaxDiscount(Deal deal, float headDiscount, float ceoDiscount, int oldDealsCount, float averageMarginality)
        {
            _logger.Log(LogLevel.Info, "Start calculating max discount...");

            var paramsDiscount = CalculateParamsDiscount(deal);
            _logger.Log(LogLevel.Info, $"Calculated params discount: {paramsDiscount}");

            var marginalityDiscount = CalculateMarginalityDiscount(deal, oldDealsCount, averageMarginality);
            _logger.Log(LogLevel.Info, $"Calculated marginality discount: {marginalityDiscount}");

            var maxDiscount = 0.0F;
            var lastDiscountParams = deal.DiscountParams.LastOrDefault();
            if (lastDiscountParams.ConsiderMarginality)
            {
                _logger.Log(LogLevel.Info, $"Consider marginality");
                maxDiscount = Math.Min(marginalityDiscount, paramsDiscount);
            }
            else
            {
                _logger.Log(LogLevel.Info, $"Don't' consider marginality");
                maxDiscount = paramsDiscount;
            }
            maxDiscount = Math.Max(maxDiscount, 0.0F);
            _logger.Log(LogLevel.Info, $"Calculated manager discount: {maxDiscount}");

            maxDiscount += headDiscount;
            _logger.Log(LogLevel.Info, $"Calculated head discount: {maxDiscount}");

            _logger.Log(LogLevel.Trace, $"Calculated max discount before brand cutting: {maxDiscount}%");
            float brandCuttingDiscount;
            try
            {
                brandCuttingDiscount = _policy.BrandPolicyDatas.FirstOrDefault(b => b.BrandName == deal.Brand).CutoffDiscountPercent;
                _logger.Log(LogLevel.Trace, $"\nDiscounts:\nbrand cutting discount: {brandCuttingDiscount}");
            }
            catch (Exception exc)
            {
                throw new Exception("Error getting discounts from policy", exc);
            }

            if (maxDiscount > brandCuttingDiscount)
                maxDiscount = brandCuttingDiscount;
            _logger.Log(LogLevel.Trace, $"Max discount after brand cutting: {maxDiscount}%");

            maxDiscount += ceoDiscount;
            _logger.Log(LogLevel.Info, $"Final discount with ceo discount: {maxDiscount}%");

            return maxDiscount;
        }

        public float CalculateMarginality(float volume, float discount, float netCost)
        {
            var sumWithDiscount = volume * (1 - discount / 100);
            var marginality = 100 * (sumWithDiscount - netCost) / sumWithDiscount;

            return marginality;
        }

        public float CalculateAverageMarginality(ICollection<float> marginalities)
        {
            if (marginalities.Count <= 0)
                throw new ArgumentException("Count of marginalitites must be higher than 0", marginalities.Count.ToString());

            return marginalities.Aggregate((x, y) => x + y) / marginalities.Count;
        }

        private float CalculateMarkup(Deal deal)
        {
            PolicyData lastPolicyData;
            try
            {
                lastPolicyData = _policy.PolicyDatas.OrderBy(pd => pd.CreateTime).Last(pd => pd.DeleteTime == null);
            }
            catch (Exception e)
            {
                throw new Exception("PolicyData was not found.", e);
            }
            var lastDiscountParams = deal.DiscountParams.Last();
            _logger.Log(LogLevel.Trace,
                $"\nParameters for calculating markup:\nprepayment:  {lastDiscountParams.Prepayment}%" +
                $"\npledged installment: {lastPolicyData.PlannedInstallment} months" +
                $"\nyearly rate: {lastPolicyData.AnnualRate}%" +
                $"\ninstallment: {lastDiscountParams.Installment} months" +
                $"\npledged prepayment: {lastPolicyData.PlannedPrepayment}%");

            if (lastDiscountParams?.Installment == 0)
                return 0;

            float markup = lastPolicyData.PlannedInstallment == 0
                ? (1 - lastDiscountParams.Prepayment / 100) * lastPolicyData.AnnualRate / 100 * (lastDiscountParams.Installment + 1) / 24
                : 1 - (24 + (lastDiscountParams.Installment + 1) * (1 - lastDiscountParams.Prepayment / 100) * lastPolicyData.AnnualRate / 100) /
                  (24 + ((lastPolicyData.PlannedInstallment + 1) * (1 - lastPolicyData.PlannedPrepayment / 100)) * lastPolicyData.AnnualRate / 100);

            return markup > 0 ? 0 : markup * 100;
        }

        private float CalculateMarginalityDiscount(Deal deal, int oldDealsCount, float averageMarginality)
        {
            float marginalityPlan = 0;
            var lastSalesPlanData = _policy.SalesPlanDatas.OrderBy(spd => spd.CreateTime).LastOrDefault(sld => sld.DeleteTime == null);
            try
            {
                marginalityPlan = 0;
                switch (deal.PartnersType)
                {
                    case PartnersType.Internet:
                        marginalityPlan = lastSalesPlanData.WholesaleMarginality;
                        break;
                    case PartnersType.InternetKey:
                        marginalityPlan = lastSalesPlanData.NetworkMarginality;
                        break;
                    case PartnersType.Key:
                        marginalityPlan = lastSalesPlanData.NetworkMarginality;
                        break;
                    case PartnersType.Network:
                        marginalityPlan = lastSalesPlanData.NetworkMarginality;
                        break;
                    case PartnersType.Wholesale:
                        marginalityPlan = lastSalesPlanData.WholesaleMarginality;
                        break;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error getting marginality plan from policy", e);
            }

            if (deal.Volume <= 0)
                throw new ArgumentException("Volume must be higher than 0", $"Volume: {deal.Volume}");

            var marginality = (1 - (deal.NetCostPure /
                                    (1 - marginalityPlan / 100 * (oldDealsCount + 1) + averageMarginality / 100 * oldDealsCount)
                                    / deal.VolumePure)) * 100;

            return marginality;
        }

        private float CalculateParamsDiscount(Deal deal)
        {
            var lastDiscountParams = deal.DiscountParams.LastOrDefault();
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);

            _logger.Log(LogLevel.Trace, $"\nSelected parameters:\npartners type:  {deal.PartnersType}" +
                                        $"\npartners repeat: {deal.Repeatedness}" +
                                        $"\norders type: {lastDiscountParams?.OrderType}" +
                                        $"\ncontracts type: {lastDiscountParams?.ContractType}" +
                                        $"\nprepayment: {lastDiscountParams?.Prepayment}" +
                                        $"\nbrand: {deal.Brand}" +
                                        $"\nbrand-mix: {deal.BrandMix}" +
                                        $"\nvolume: {deal.Volume}");

            float partnersTypeDiscount, partnersRepetitionTypeDiscount, ordersTypeDiscount, contractsTypeDiscount, brandMixDiscount, volumeDiscount, prepaymentDiscount;
            bool isRequiredBrandVolume;
            try
            {
                partnersTypeDiscount = GetPartnersTypeDiscount(deal);
                partnersRepetitionTypeDiscount = deal.Repeatedness ? lastPolicyData.RepeatedPartnerDiscount : 0;
                ordersTypeDiscount = GetOrderTypeDiscount(deal);
                contractsTypeDiscount = GetContractTypeDiscount(deal);
                brandMixDiscount = GetBrandMixDiscount(deal);
                volumeDiscount = GetVolumeDiscount(deal);
                prepaymentDiscount = GetPrepaymentDiscount(deal);

                var minBrandVolume = _policy.BrandPolicyDatas.FirstOrDefault(p => p.BrandName == deal.Brand).Volume;
                isRequiredBrandVolume = !(minBrandVolume * 1000 > deal.Volume);
            }
            catch (Exception e)
            {
                throw new Exception("Error getting discounts from policy", e);
            }

            _logger.Log(LogLevel.Trace, $"\nDiscounts:\npartners type discount:  {partnersTypeDiscount}" +
                                        $"\npartners repetition type discount: {partnersRepetitionTypeDiscount}" +
                                        $"\norders type discount: {ordersTypeDiscount}" +
                                        $"\ncontracts type discount: {contractsTypeDiscount}" +
                                        $"\nbrand-mix discount: {brandMixDiscount}" +
                                        $"\nprepayment discount: {prepaymentDiscount}" +
                                        $"\nvolume discount: {volumeDiscount}");

            float markup;
            try
            {
                _logger.Log(LogLevel.Info, "Start calculating markup...");
                markup = CalculateMarkup(deal);
                _logger.Log(LogLevel.Info, $"Calculated markup: {markup}%");
            }
            catch (Exception e)
            {
                throw new Exception("Error calculating markup.", e);
            }

            var maxDiscount = partnersTypeDiscount + partnersRepetitionTypeDiscount + ordersTypeDiscount + contractsTypeDiscount + brandMixDiscount + prepaymentDiscount + volumeDiscount + markup;
            if (!isRequiredBrandVolume)
                maxDiscount += lastPolicyData.MarkupForMismatchOfVolume;

            return maxDiscount;
        }


        private float GetPrepaymentDiscount(Deal deal)
        {
            var lastDiscountParams = deal.DiscountParams.LastOrDefault();
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            float discount = 0;

            var prepayments = JsonConvert.DeserializeObject<List<ParamsDiscount>>(lastPolicyData.PrepaymentDiscount).OrderByDescending(b => b.Value);

            foreach (var prepayment in prepayments)
                if (lastDiscountParams.Prepayment >= prepayment.Value)
                    return prepayment.Discount;

            return discount;
        }

        private float GetPartnersTypeDiscount(Deal deal)
        {
            float discount = 0;

            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            switch (deal.PartnersType)
            {
                case PartnersType.Internet:
                    discount = lastPolicyData.InternetPartnerImportanceDiscount;
                    break;
                case PartnersType.InternetKey:
                    discount = lastPolicyData.InternetKeyPartnerImportanceDiscount;
                    break;
                case PartnersType.Key:
                    discount = lastPolicyData.KeyPartnerImportanceDiscount;
                    break;
                case PartnersType.Network:
                    discount = lastPolicyData.NetworkPartnerImportanceDiscount;
                    break;
                case PartnersType.Wholesale:
                    discount = lastPolicyData.WholesalePartnerImportanceDiscount;
                    break;
            }

            return discount;
        }

        private float GetOrderTypeDiscount(Deal deal)
        {
            float discount = 0;
            var lastDiscountParams = deal.DiscountParams.LastOrDefault();
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            switch (lastDiscountParams.OrderType)
            {
                case OrderType.CurrentFreeWarehouse:
                    discount = lastPolicyData.FreeWarehouseCurrentOrderDiscount;
                    break;
                case OrderType.PastFreeWarehouse:
                    discount = lastPolicyData.FreeWarehousePastOrderDiscount;
                    break;
                case OrderType.PreOrder:
                    discount = lastPolicyData.PreOrderDiscount;
                    break;
            }

            return discount;
        }


        private float GetContractTypeDiscount(Deal deal)
        {
            float discount = 0;
            var lastDiscountParams = deal.DiscountParams.LastOrDefault();
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            switch (lastDiscountParams.ContractType)
            {
                case ContractType.Comission:
                    discount = lastPolicyData.Commission;
                    break;
                case ContractType.Sale:
                    discount = lastPolicyData.PurchaseAndSaleDiscount;
                    break;
            }

            return discount;
        }

        private float GetBrandMixDiscount(Deal deal)
        {
            float discount = 0;
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            var brandMixes = JsonConvert.DeserializeObject<List<ParamsDiscount>>(lastPolicyData.BrandMixDiscount).OrderByDescending(b => b.Value);

            foreach (var brandMix in brandMixes)
                if (deal.BrandMix >= brandMix.Value)
                    return brandMix.Discount;

            return discount;
        }

        private float GetVolumeDiscount(Deal deal)
        {
            float volumeDiscount = 0;
            var lastPolicyData = _policy.PolicyDatas.OrderBy(p => p.CreateTime).LastOrDefault(pd => pd.DeleteTime == null);
            var volumes = JsonConvert.DeserializeObject<List<ParamsDiscount>>(lastPolicyData.VolumeDiscount).OrderByDescending(b => b.Value);

            foreach (var volume in volumes)
                if ((deal.Volume * 72 / 1000) >= volume.Value)
                    return volume.Discount;

            return volumeDiscount;
        }

        internal class ParamsDiscount
        {
            public int Value { get; set; }

            public float Discount { get; set; }
        }
    }
}