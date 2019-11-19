using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Company.Calculator.Clients.v2;
using Company.Calculator.Options;
using Company.Common.Models.Pim;
using Company.Common.Models.Seasons;
using NLog;
using Company.Common.Options;

namespace Company.Calculator.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class NetCostController : Controller
    {
        private IConfiguration _configuration { get; }
        private AttributesIdsOptions _attributesIdsOptions { get; }
        private CoefficientsOptions _coeffcientOptions { get; }
        private ListsIdsOption _listsIdsOption { get; }
        private readonly Logger _logger;

        public NetCostController(IConfiguration configuration,
            IOptions<AttributesIdsOptions> attributesIdsOptions,
            IOptions<ListsIdsOption> listsIdsOption,
            IOptions<CoefficientsOptions> coeffcientOptions)
        {
            _configuration = configuration;
            _attributesIdsOptions = attributesIdsOptions.Value;
            _coeffcientOptions = coeffcientOptions.Value;
            _listsIdsOption = listsIdsOption.Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditNetCost([FromRoute] int id, [FromQuery] bool recalculate, [FromBody] Product product, [FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit net cost method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            if (brandId == 0 || seasonId == 0)
            {
                _logger.Log(LogLevel.Error, "There are no brandId or seasonId in query.");
                return BadRequest("There are no brandId or seasonId in query.");
            }

            var seasonsClient = new HttpSeasonsMSClient();
            var httpClient = new HttpPimClient();

            _logger.Log(LogLevel.Trace, $"Getting logistic by seasonId='{seasonId}' and brandId='{brandId}'...");
            var logistic = seasonsClient.GetLogisticBySeasonAndBrandValues(
                seasonId,
                brandId,
                HttpContext);

            if (logistic == null)
            {
                _logger.Log(LogLevel.Error, $"Logistic with seasonId='{seasonId}' and brandId='{brandId}' not found");
                return NotFound($"Logistic with seasonId='{seasonId}' and brandId='{brandId}' not found");
            }
            _logger.Log(LogLevel.Trace, $"Logistic by seasonId='{seasonId}' and brandId='{brandId}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting list value...");
            var vatListValue = httpClient.GetListValue(product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Vat).ListValueId.Value, HttpContext);
            _logger.Log(LogLevel.Trace, "List value successfully fetched");

            _logger.Log(LogLevel.Trace, "Checking Fob...");
            var isFobVatExist = product.AttributeValues.Any(p => p.AttributeId == _attributesIdsOptions.Fob && p.NumValue != null) && vatListValue != null;
            if (isFobVatExist)
            {
                _logger.Log(LogLevel.Trace, "Fob are checked");

                _logger.Log(LogLevel.Trace, "Getting fob value...");
                var fobValue = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Fob).NumValue;
                _logger.Log(LogLevel.Trace, "Fob successfully fetched");

                _logger.Log(LogLevel.Trace, "Calculation logistic value...");
                var logisticValue = LogisticValueWithoutFob(logistic) * fobValue;
                _logger.Log(LogLevel.Trace, "Calculation logistic value is finished");

                _logger.Log(LogLevel.Trace, "Calculation other value...");
                var other = fobValue * logistic.OtherAdditional / logistic.MoneyVolume;
                _logger.Log(LogLevel.Trace, "Calculation other value is finished");

                _logger.Log(LogLevel.Trace, "Getting tax value...");
                var taxValue = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Tax).NumValue;
                _logger.Log(LogLevel.Trace, "Tax successfully fetched");

                _logger.Log(LogLevel.Trace, "Parsing list value...");
                var vatValue = double.Parse(vatListValue.Value);
                _logger.Log(LogLevel.Trace, "Parsing list value is finished");

                _logger.Log(LogLevel.Trace, "Calculation landing cost pure...");
                var landingCostPure = fobValue + logisticValue + other;
                if (taxValue.HasValue)
                    landingCostPure += taxValue;
                _logger.Log(LogLevel.Trace, "Calculation logistic value is finished");

                _logger.Log(LogLevel.Trace, "Calculation landing cost...");
                var landingCost = landingCostPure * (1 + vatValue / 100);
                _logger.Log(LogLevel.Trace, "Calculation landing cost is finished");

                _logger.Log(LogLevel.Trace, "Recalculation attribute values...");
                RecalculateAttributeValue(_attributesIdsOptions.Lc, landingCost, product);
                RecalculateAttributeValue(_attributesIdsOptions.Lcp, landingCostPure, product);
                _logger.Log(LogLevel.Trace, "Recalculation attribute values is finished");


                if (recalculate)
                {
                    _logger.Log(LogLevel.Trace, "Calculation BWP...");
                    var bwp = landingCost * _coeffcientOptions.Bwp;
                    _logger.Log(LogLevel.Trace, "Calculation BWP is finished");

                    _logger.Log(LogLevel.Trace, "Calculation RRC...");
                    var rrc = bwp * _coeffcientOptions.Rrc;
                    _logger.Log(LogLevel.Trace, "Calculation RRC is finished");

                    _logger.Log(LogLevel.Trace, "Calculation BWP pure...");
                    var bwpPure = bwp - landingCost + landingCostPure;
                    _logger.Log(LogLevel.Trace, "Calculation BWP pure is finished");

                    _logger.Log(LogLevel.Trace, "Calculation RRC pure...");
                    var rrcPure = rrc - landingCost + landingCostPure;
                    _logger.Log(LogLevel.Trace, "Calculation RRC pure is finished");

                    _logger.Log(LogLevel.Trace, "Recalculation attribute values...");
                    RecalculateAttributeValue(_attributesIdsOptions.Bwp, bwp, product);
                    RecalculateAttributeValue(_attributesIdsOptions.BwpPure, bwpPure, product);
                    RecalculateAttributeValue(_attributesIdsOptions.Rrc, rrc, product);
                    RecalculateAttributeValue(_attributesIdsOptions.RrcPure, rrcPure, product);
                    _logger.Log(LogLevel.Trace, "Recalculation attribute values is finished");
                }
            }
            else
            {
                _logger.Log(LogLevel.Info, "Fob are not exist");
            }

            _logger.Log(LogLevel.Trace, $"Posting the product with id='{product.Id}'...");
            var result = await httpClient.PostProduct(product, HttpContext);
            _logger.Log(LogLevel.Trace, $"Posting the product with id='{product.Id}' is finished");

            _logger.Log(LogLevel.Debug, "Edit net cost method is over.");
            return StatusCode((int) result.StatusCode, JObject.Parse(result.Content.ReadAsStringAsync().Result));
        }

        [HttpPut("properties")]
        public async Task<IActionResult> EditMultipleNetCost([FromQuery] bool recalculate, [FromBody] List<AttributeValue> attributeValues, [FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit multiple net cost method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            if (brandId == 0 || seasonId == 0)
            {
                _logger.Log(LogLevel.Error, "There are no brandId or seasonId in query.");
                return BadRequest("There are no brandId or seasonId in query.");
            }

            var seasonsClient = new HttpSeasonsMSClient();
            var httpClient = new HttpPimClient();
            var products = new List<Product>();

            _logger.Log(LogLevel.Trace, "Getting product ids...");
            var productsIds = attributeValues.Select(x => x.ProductId).Distinct().ToList();
            _logger.Log(LogLevel.Trace, $"Product ids {String.Join(',', productsIds)} successfully fetched");
            try
            {
                _logger.Log(LogLevel.Trace, $"Getting products with ids='{String.Join(',', productsIds)}'...");
                products = await httpClient.GetProductCmpyIds(productsIds, HttpContext);
                _logger.Log(LogLevel.Trace, $"Products with ids='{String.Join(',', productsIds)}' successfully fetched");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error: {e.Message}");
                return BadRequest(e.Message);
            }


            var attributesIds = attributeValues.Select(av => av.AttributeId).Distinct();

            var productsDict = products.Where(p => productsIds.Contains(p.Id)).ToDictionary(p => p.Id);
            var categoriesIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId.Value).Distinct();

            var attributeCategories = await httpClient.GetAttributesCategories(categoriesIds.ToList(), attributesIds.ToList(), HttpContext);

            attributeValues = attributeValues.Where(av =>
                attributeCategories.Any(ac => ac.ModelLevel == productsDict[av.ProductId].ModelLevel &&
                                              ac.CategoryId == productsDict[av.ProductId].CategoryId &&
                                              ac.AttributeId == av.AttributeId)).ToList();         

            _logger.Log(LogLevel.Trace, "Getting logistic by seasonId='{seasonListValueId.Value}' and brandId='{brandListValueId.Value}'...");
            var logistic = seasonsClient.GetLogisticBySeasonAndBrandValues(
                seasonId,
                brandId,
                HttpContext);

            if (logistic == null)
            {
                _logger.Log(LogLevel.Error, $"Logistic with seasonId='{seasonId}' and brandId='{brandId}' not found");
                return NotFound($"Logistic with seasonId='{seasonId}' and brandId='{brandId}' not found");
            }
            _logger.Log(LogLevel.Trace, $"Logistic with seasonId='{seasonId}' and brandId='{brandId}' successfully fetched");

            _logger.Log(LogLevel.Trace, $"Getting lists dto by tnved='{_listsIdsOption.Tnved}' and vat='{_listsIdsOption.Vat}'...");
            var listsDto = await httpClient.GetLists(new List<int> {_listsIdsOption.Tnved, _listsIdsOption.Vat}, HttpContext);
            _logger.Log(LogLevel.Trace, "Lists dto successfully fetched");

            var vatList = listsDto.First(l => l.Id == _listsIdsOption.Vat).ListValues;

            _logger.Log(LogLevel.Trace, $"Calculation attribute values of products...");
            try
            {
                foreach (var product in products)
                {

                    product.AttributeValues.Add(attributeValues.First(p => p.ProductId == product.Id));


                    double? fob = null;
                    double? tax = null;
                    double? vat = null;
                    foreach (var property in product.AttributeValues)
                    {
                        if (property.AttributeId == _attributesIdsOptions.Fob && property.NumValue != null)
                        {
                            fob = property.NumValue;
                        }
                        else if (property.AttributeId == _attributesIdsOptions.Tax && property.NumValue != null)
                        {
                            tax = property.NumValue;
                        }
                        else if (property.AttributeId == _attributesIdsOptions.Vat && property.ListValueId != null)
                        {
                            var vatValue = vatList.FirstOrDefault(l => l.Id == property.ListValueId)?.Value;
                            if (vatValue != null)
                                vat = double.Parse(vatValue);
                        }
                    }


                    var isFobVatExist = vat != null && fob != null;

                    if (isFobVatExist)
                    {
                        attributeValues.AddRange(CalculateLc(logistic, product.Id, fob.Value, tax, vat.Value, out var lc, out var lcp));

                        if (recalculate)
                            attributeValues.AddRange(CalculateBpwRrc(product.Id, lc, lcp));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on calculation attribute values of products. Error: {e.Message}");
                return BadRequest("Error on calculation attribute values of products");
            }
            _logger.Log(LogLevel.Trace, "Calculation attribute values of products is finished");

            _logger.Log(LogLevel.Trace, "Editing product properties...");
            var result = await httpClient.EditProductProperties(attributeValues, HttpContext);
            _logger.Log(LogLevel.Trace, "Product properties is edited");

            _logger.Log(LogLevel.Debug, "Edit multiple net cost method is over.");
            return StatusCode((int) result.StatusCode);
        }


        private List<AttributeValue> CalculateLc(Logistic logistic, int productId, double fob, double? tax, double vat, out double lc, out double lcp)
        {
            var logisticValue = LogisticValueWithoutFob(logistic) * fob;
            var other = fob * logistic.OtherAdditional / logistic.MoneyVolume;


            lcp = fob + logisticValue + other;
            if (tax.HasValue)
                lcp += tax.Value;

            lc = lcp * (1 + vat / 100);

            return new List<AttributeValue>
            {
                CreateAttributeValue(_attributesIdsOptions.Lc, lc, productId),
                CreateAttributeValue(_attributesIdsOptions.Lcp, lcp, productId),
            };
        }

        private List<AttributeValue> CalculateBpwRrc(int productId, double landingCost, double landingCostPure)
        {
            var bwp = landingCost * _coeffcientOptions.Bwp;
            var rrc = bwp * _coeffcientOptions.Rrc;
            var bwpPure = bwp - landingCost + landingCostPure;
            var rrcPure = rrc - landingCost + landingCostPure;


            return new List<AttributeValue>
            {
                CreateAttributeValue(_attributesIdsOptions.Bwp, bwp, productId),
                CreateAttributeValue(_attributesIdsOptions.BwpPure, bwpPure, productId),
                CreateAttributeValue(_attributesIdsOptions.Rrc, rrc, productId),
                CreateAttributeValue(_attributesIdsOptions.RrcPure, rrcPure, productId),
            };
        }

        private AttributeValue CreateAttributeValue(int? attributeId, double? attributeValue, int productId)
        {
            return new AttributeValue()
            {
                AttributeId = attributeId.Value,
                NumValue = attributeValue,
                ProductId = productId
            };
        }

        private void RecalculateAttributeValue(int? attributeId, double? attributeValue, Product product)
        {
            var attrValue = product.AttributeValues.Where(av => av.AttributeId == attributeId).OrderByDescending(av => av.CreateTime).FirstOrDefault();
            if (attrValue != null)
            {
                attrValue.NumValue = attributeValue;
            }
            else
            {
                product.AttributeValues.Add(new AttributeValue()
                {
                    AttributeId = attributeId.Value,
                    NumValue = attributeValue,
                    ProductId = product.Id
                });
            }
        }

        private float LogisticValueWithoutFob(Logistic l)
            => l.Supplies.Sum(s => (s.TransportCost * (s.RiskCoefficient / 100 + 1) + s.BrokerCost + s.WtsCost + s.Other) * s.BatchesCount) * (l.Insurance / 100 + 1) * l.AdditionalFactor / l.MoneyVolume;
    }
}