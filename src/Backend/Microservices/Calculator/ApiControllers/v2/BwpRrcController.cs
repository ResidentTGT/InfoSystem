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
using Company.Common.Enums;
using Company.Common.Models.Pim;
using NLog;
using Company.Common.Options;

namespace Company.Calculator.ApiControllers.v2
{
    [Route("v2/bwp-rrc")]
    public class BwpRrcController : Controller
    {
        private IConfiguration _configuration { get; }
        private AttributesIdsOptions _attributesIdsOptions;
        private readonly Logger _logger;

        public BwpRrcController(IConfiguration configuration, IOptions<AttributesIdsOptions> attributesIdsOptions)
        {
            _configuration = configuration;
            _attributesIdsOptions = attributesIdsOptions.Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditSingleBwpAndRrc([FromRoute] int id, [FromBody] Product product)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit single bwp and rrc method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, "Getting landing cost pure...");
            var landingCostPure = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Lcp).NumValue;
            _logger.Log(LogLevel.Trace, $"Landing cost pure ='{landingCostPure}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting landing cost...");
            var landingCost = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Lc).NumValue;
            _logger.Log(LogLevel.Trace, "Landing cost successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting BWP...");
            var bwp = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Bwp).NumValue;
            _logger.Log(LogLevel.Trace, $"BWP='{bwp}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting RRC...");
            var rrc = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Rrc).NumValue;
            _logger.Log(LogLevel.Trace, $"RRC='{rrc}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Calculation BWP pure...");
            var bwpPure = bwp - landingCost + landingCostPure;
            _logger.Log(LogLevel.Trace, $"BWP='{bwpPure}' pure is calculated");

            _logger.Log(LogLevel.Trace, "Calculation RRC pure...");
            var rrcPure = rrc - landingCost + landingCostPure;
            _logger.Log(LogLevel.Trace, $"RRC pure='{rrcPure}' is calculated");

            _logger.Log(LogLevel.Trace, "Recalculation attribute values...");
            RecalculateAttributeValue(_attributesIdsOptions.BwpPure, bwpPure, product);
            RecalculateAttributeValue(_attributesIdsOptions.RrcPure, rrcPure, product);
            _logger.Log(LogLevel.Trace, "Recalculation attribute values is finished");

            var httpClient = new HttpPimClient();

            _logger.Log(LogLevel.Trace, "Posting the product...");
            var result = await httpClient.PostProduct(product, HttpContext);
            _logger.Log(LogLevel.Trace, "Posting the product is finished");

            _logger.Log(LogLevel.Debug, "Edit single bwp and rrc method is over.");
            return StatusCode((int) result.StatusCode, JObject.Parse(result.Content.ReadAsStringAsync().Result));
        }

        [HttpPut("properties")]
        public async Task<IActionResult> EditMultipleBwpAndRrc(
            [FromQuery] RetailAttribute pin,
            [FromQuery] RetailAttribute editAttr,
            [FromBody] List<AttributeValue> attributeValues)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit multiple bwp and rrc method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, "Getting attribute id...");
            var attributeId = editAttr == RetailAttribute.Bwp ? _attributesIdsOptions.Bwp : (editAttr == RetailAttribute.Rrc ? _attributesIdsOptions.Rrc : 0);
            _logger.Log(LogLevel.Trace, "Attribute id successfully fetched");


            var httpClient = new HttpPimClient();

            _logger.Log(LogLevel.Trace, "Getting product ids...");
            var productsIds = attributeValues.Select(p => p.ProductId).ToList();
            _logger.Log(LogLevel.Trace, "Product ids successfully fetched");

            _logger.Log(LogLevel.Trace, "Getting products by ids...");
            var products = await httpClient.GetProductCmpyIds(productsIds, HttpContext);
            _logger.Log(LogLevel.Trace, "Products by ids successfully fetched");

            _logger.Log(LogLevel.Trace, "Calculation BWP, RRC, RRC pure, BWP pure...");

            var attributesIds = attributeValues.Select(av => av.AttributeId).Distinct().ToList();

            var productsDict = products.Where(p => productsIds.Contains(p.Id)).ToDictionary(p => p.Id);
            var categoriesIds = products.Where(p => p.CategoryId.HasValue).Select(p => p.CategoryId.Value).Distinct().ToList();

            var attributeCategories = await httpClient.GetAttributesCategories(categoriesIds, attributesIds, HttpContext);

            _logger.Log(LogLevel.Trace, "Creating product properties...");
            var productProperties = attributeValues.Where(av =>
                    attributeCategories.Any(ac => ac.ModelLevel == productsDict[av.ProductId].ModelLevel &&
                                                  ac.CategoryId == productsDict[av.ProductId].CategoryId &&
                                                  ac.AttributeId == av.AttributeId))
                .Select(a => new AttributeValue()
                {
                    AttributeId = attributeId.Value,
                    NumValue = a.NumValue,
                    ProductId = a.ProductId
                })
                .ToList();

            _logger.Log(LogLevel.Trace, "Creating product properties is finished");

            try
            {
                foreach (var product in products)
                {

                    var landingCostPure = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Lcp)?.NumValue;
                    var landingCost = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Lc)?.NumValue;

                    var bwp = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Bwp)?.NumValue;
                    var rrc = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Rrc)?.NumValue;

                    var mustBeWithValueForCalculation = new[] { landingCostPure, landingCost, bwp, rrc };
                    if (mustBeWithValueForCalculation.Contains(null))
                        continue;

                    switch (editAttr)
                    {
                        case RetailAttribute.Bwp:

                            if (pin == RetailAttribute.RrcK)
                            {
                                var editedBwp = productProperties.First(p => p.ProductId == product.Id)?.NumValue;
                                AddNewAttributeValue(_attributesIdsOptions.Rrc, editedBwp * (rrc / bwp), product.Id, productProperties);
                            }
                            break;

                        case RetailAttribute.BwpK:

                            var bwpK = productProperties.First(p => p.ProductId == product.Id)?.NumValue;
                            AddNewAttributeValue(_attributesIdsOptions.Bwp, bwpK * landingCost, product.Id, productProperties);

                            if (pin == RetailAttribute.RrcK)
                                AddNewAttributeValue(_attributesIdsOptions.Rrc, (bwpK * landingCost) * (rrc / bwp), product.Id, productProperties);

                            break;

                        case RetailAttribute.Rrc:

                            if (pin == RetailAttribute.RrcK)
                            {
                                var editedRrc = productProperties.First(p => p.ProductId == product.Id)?.NumValue;

                                AddNewAttributeValue(_attributesIdsOptions.Bwp, editedRrc / (rrc / bwp), product.Id, productProperties);
                            }
                            break;

                        case RetailAttribute.RrcK:

                            var rrcK = productProperties.First(p => p.ProductId == product.Id)?.NumValue;
                            if (pin == RetailAttribute.Rrc)
                                AddNewAttributeValue(_attributesIdsOptions.Bwp, rrc / rrcK, product.Id, productProperties);

                            if (pin == RetailAttribute.Bwp)
                                AddNewAttributeValue(_attributesIdsOptions.Rrc, bwp * rrcK, product.Id, productProperties);

                            break;
                    }

                    var bwpPure = (productProperties.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Bwp)?.NumValue ?? bwp) - landingCost + landingCostPure;
                    var rrcPure = (productProperties.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Rrc)?.NumValue ?? rrc) - landingCost + landingCostPure;

                    AddNewAttributeValue(_attributesIdsOptions.BwpPure, bwpPure, product.Id, productProperties);
                    AddNewAttributeValue(_attributesIdsOptions.RrcPure, rrcPure, product.Id, productProperties);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on calculation BWP, RRC, RRC pure, BWP pure. Error: {e.Message}");
                return BadRequest("Error on calculation BWP, RRC, RRC pure, BWP pure");
            }
            _logger.Log(LogLevel.Trace, "Calculation BWP, RRC, RRC pure, BWP pure is finished");

            _logger.Log(LogLevel.Trace, "Editing product properties...");
            var result = await httpClient.EditProductProperties(productProperties.Where(p => p.AttributeId != 0).ToList(), HttpContext);
            _logger.Log(LogLevel.Trace, "Product properties is edited");

            _logger.Log(LogLevel.Debug, "Edit multiple bwp and rrc method is over.");
            return StatusCode((int) result.StatusCode);
        }

        private void AddNewAttributeValue(int? attributeId, double? attributeValue, int productId, List<AttributeValue> productPropertieses)
        {
            productPropertieses.Add(new AttributeValue()
            {
                AttributeId = attributeId.Value,
                NumValue = attributeValue,
                ProductId = productId
            });
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
    }
}