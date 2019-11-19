using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;
using Microsoft.EntityFrameworkCore;
using Company.Pim.Helpers.v2;
using NLog;
using Company.Common.Extensions;
using Company.Common.Requests.Pim;

namespace Company.Pim.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class TradeManagementController : Controller
    {
        private readonly PimContext _context;
        private readonly TransformModelHelpers _transformModelHelper;
        private readonly Logger _logger;

        public TradeManagementController(PimContext context, TransformModelHelpers transformModelHelper)
        {
            _context = context;
            _transformModelHelper = transformModelHelper;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductXml([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, "Start geting product xml...");

            _logger.Log(LogLevel.Trace, $"Getting product by id='{id}'...");
            var product = await _context.Products.Where(p => p.DeleteTime == null)
                .Include(p => p.ProductFiles)
                .Include(p => p.Category.AttributeCategories)
                .Include(p => p.AttributeValues)
                .ThenInclude(avl => avl.Attribute)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (product == null)
            {
                _logger.Log(LogLevel.Error, $"Product with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Product '{product.Name}' successfully received");

            _logger.Log(LogLevel.Trace, "Getting attribute values...");
            product.AttributeValues = product.AttributeValues.Where(av => product.Category.AttributeCategories.Any(ac => ac.AttributeId == av.AttributeId))
                .GroupBy(a => a.AttributeId)
                .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                .ToList();
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Debug, "Geting product xml is finished.");
            return Ok(_transformModelHelper.TransformProduct(product));
        }

        [HttpGet]
        public async Task<IActionResult> GetProductCmpySkuWithAttributes()
        {
            _logger.Log(LogLevel.Debug, "Start geting products by sku with attributes...");

            _logger.Log(LogLevel.Trace, "Getting skus from request query...");
            var skus = Request.Query["skus"].ToString().Split(',').ToArray();
            _logger.Log(LogLevel.Trace, $"Skus '{String.Join(',', skus)}' successfully received");

            _logger.Log(LogLevel.Trace, "Getting attributes ids from request query...");
            var attributesIds = Request.Query["attributesIds"].Count == 0
                ? new int[0]
                : Request.Query["attributesIds"].ToString().Split(',').Select(int.Parse).ToArray();
            _logger.Log(LogLevel.Trace, "Attributes ids successfully received");

            _logger.Log(LogLevel.Trace, "Getting products...");
            var products = await _context.Products.Where(p => p.DeleteTime == null)
                .Include(p => p.AttributeValues)
                .ThenInclude(avl => avl.Attribute.AttributeCategories)
                .Include(p => p.ParentProduct.AttributeValues)
                .ThenInclude(avl => avl.Attribute.AttributeCategories)
                .Include(p => p.ParentProduct.ParentProduct.AttributeValues)
                .ThenInclude(avl => avl.Attribute.AttributeCategories)
                .Where(p => skus.Contains(p.Sku))
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Getting attribute values...");
            foreach (var product in products)
                if (attributesIds.Any())
                    product.AttributeValues = GetAllAttributeValues(product, attributesIds);
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            var categories = await _context.Categories.Where(c => c.DeleteTime == null).ToListAsync();
            return Ok(products.Select(p =>
                new
                {
                    Product = _transformModelHelper.TransformProduct(p, false, false),
                    CategoryTreeString = GetStringCategoryTree(p.CategoryId, categories)
                }));
        }


        [HttpGet("sku")]
        [Produces("application/json")]
        public async Task<IActionResult> GetProductSkuCmpyGUID([FromQuery] string guidN, [FromQuery] string guidX)
        {
            return Ok((await _context.Products
                    .FirstOrDefaultAsync(
                        p => p.AttributeValues.Any(av => av.AttributeId == 740 && av.StrValue == guidN)
                            && p.AttributeValues.Any(av => av.AttributeId == 741 && av.StrValue == guidX)))?.Sku);
        }

        [HttpPost]
        public async Task<IActionResult> GetProductCmpySkuWithAttributesPost([FromBody] ProductCmpySkusAndAttributesIdsRequest request)
        {
            _logger.Log(LogLevel.Debug, "Start geting products by sku with attributes post...");

            _logger.Log(LogLevel.Trace, $"Getting products by skus '{String.Join(',', request.Skus)}'...");
            var products = await _context.Products.Where(p => p.DeleteTime == null && request.Skus.Contains(p.Sku))
                .Include(p => p.ParentProduct.ParentProduct)
                .Include(p => p.SubProducts)
                .ThenInclude(sp => sp.SubProducts)
                .ToListAsync();

            _logger.Log(LogLevel.Trace, "Products successfully received");

            var treeObjectHelper = new TreeObjectHelper();

            var parentProducts = treeObjectHelper.GetPropertyFromTreeObject(products, p => p,
                p => p.ParentProduct != null ? new List<Product> { p.ParentProduct } : new List<Product>()
            ).Where(p => p.ParentId == null).GroupBy(p => p.Id).Select(p => p.First()).ToList();

            products = treeObjectHelper.GetPropertyFromTreeObject(parentProducts, p => p, p => p.SubProducts);

            var productsCategoriesIds = products.Select(p => p.CategoryId).Distinct();
            var productsIds = products.Select(p => p.Id).Distinct();
            _logger.Log(LogLevel.Trace, "Loading attributeValues...");
            await _context.AttributeValues
                .Include(av => av.Attribute)
                .Where(av => request.AttributesIds.Contains(av.AttributeId) &&
                             productsIds.Contains(av.ProductId) &&
                             av.Attribute.DeleteTime == null)
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "Loading attributeCategories...");
            await _context.AttributeCategories
                .Where(ac => request.AttributesIds.Contains(ac.AttributeId) &&
                             productsCategoriesIds.Contains(ac.CategoryId) &&
                             ac.Attribute.DeleteTime == null)
                .LoadAsync();

            _logger.Log(LogLevel.Trace, "Filtering attribute values...");
            foreach (var product in products)
            {
                product.AttributeValues = product.AttributeValues
                    .GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            }
            _logger.Log(LogLevel.Trace, "Attribute values successfully filtered");

            _logger.Log(LogLevel.Trace, "Getting categories...");

            var categories = await _context.Categories.Where(c => c.DeleteTime == null).ToListAsync();
            _logger.Log(LogLevel.Trace, "Categories successfully received");

            _logger.Log(LogLevel.Debug, "Geting products by sku with attributes post is finished.");

            return Ok(products.Select(p =>
                new
                {
                    Product = _transformModelHelper.TransformProduct(p, false, false),
                    CategoryTreeString = GetStringCategoryTree(p.CategoryId, categories)
                }));
        }

        private string GetStringCategoryTree(int? categoryId, List<Category> categories)
        {
            if (categoryId != null)
            {
                var category = categories.FirstOrDefault(c => c.Id == categoryId);
                var categoryNames = new List<string>();

                while (category != null)
                {
                    categoryNames.Add(category.Name);
                    category = category.ParentId.HasValue
                        ? categories.First(c => c.Id == category.ParentId)
                        : null;
                }

                categoryNames.Reverse();

                return String.Join('$', categoryNames) + "$";
            }

            return "";
        }
        private List<AttributeValue> GetAllAttributeValues(Product product, int[] attributesIds)
        {
            var attrValues = GetLevelAttributeValues(product, attributesIds);

            if (product.ParentProduct != null)
            {
                attrValues.AddRange(GetLevelAttributeValues(product.ParentProduct, attributesIds));

                if (product.ParentProduct.ParentProduct != null)
                    attrValues.AddRange(GetLevelAttributeValues(product.ParentProduct.ParentProduct, attributesIds));
            }

            return attrValues;
        }

        private List<AttributeValue> GetLevelAttributeValues(Product product, int[] attributesIds) =>
           product.AttributeValues
            .Where(av => attributesIds.Contains(av.AttributeId)
                         && av.Attribute.DeleteTime == null
                         && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId
                         //&& ac.ModelLevel == product.ModelLevel
                         ))
                   .GroupBy(a => a.AttributeId)
                   .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                   .ToList();
    }
}