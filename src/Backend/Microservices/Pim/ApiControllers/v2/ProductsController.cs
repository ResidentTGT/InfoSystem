using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using Company.Common;
using Company.Common.Extensions;
using Company.Common.Models;
using Company.Common.Enums;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Pim.Client.v2;
using Company.Pim.Helpers.v2;
using Microsoft.Net.Http.Headers;
using NpgsqlTypes;
using Company.Common.Models.Seasons;
using Company.Pim.Options;
using Microsoft.Extensions.Options;
using Attribute = System.Attribute;

namespace Company.Pim.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class ProductsController : Controller
    {
        private readonly PimContext _context;
        private readonly Logger _logger;
        private readonly SkuGenerator _skuGenerator;
        private IWebApiCommunicator _webApiCommunicator { get; set; }
        private ISeasonsMsCommunicator _seasonsMsCommunicator { get; set; }
        private CurrentSeasonOptions _currentSeasonOptions { get; set; }
        private TransformModelHelpers _transformModelHelper { get; set; }
        private readonly TreeObjectHelper _treeObjectHelper;
        private const string SkuParametr = "SKU";
        private const string NameParametr = "НАИМЕНОВАНИЕ";
        private const string ImportsParametr = "ИМПОРТ";

        public ProductsController(PimContext context,
            IWebApiCommunicator httpWebApiCommunicator,
            ISeasonsMsCommunicator seasonsMsCommunicator,
            IOptions<CurrentSeasonOptions> currentSeasonOptions,
            SkuGenerator skuGenerator,
            TransformModelHelpers transformModelHelpers,
            TreeObjectHelper treeObjectHelper)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();
            _skuGenerator = skuGenerator;
            _transformModelHelper = transformModelHelpers;
            _webApiCommunicator = httpWebApiCommunicator;
            _seasonsMsCommunicator = seasonsMsCommunicator;
            _currentSeasonOptions = currentSeasonOptions.Value;
            _treeObjectHelper = treeObjectHelper;
        }

        [HttpPost("test")]
        public string Test()
        {
            var doc = new XmlDocument();
            doc.Load(HttpContext.Request.Body);

            return "afsa";
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id, [FromQuery] bool withParents = false)
        {
            _logger.Log(LogLevel.Debug, $"Start getting product by id='{id}'...");
            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _webApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, "Getting products with subproducts...");
            IQueryable<Product> query = _context.Products.Where(p => p.DeleteTime == null)
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts);
            _logger.Log(LogLevel.Trace, "Products and subproducts successfully received");

            if (withParents)
                query = query.Include(p => p.ParentProduct.ParentProduct);

            _logger.Log(LogLevel.Info, $"Getting products with id='{id}'...");
            var product = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (product == null)
            {
                _logger.Log(LogLevel.Error, $"Product with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Product with id='{id}' successfully received");

            _logger.Log(LogLevel.Trace, $"Getting subproducts of product with id='{id}'...");
            // Достаем все Idшники продуктов
            var productsIds = new List<int>();
            productsIds.AddRange(GetPropertiesFromProductTree(product, p => p.Id));
            _logger.Log(LogLevel.Trace, $"Subproducts of product with id='{id}' successfully received");

            // Также Id родительских если нужны
            if (withParents && product.ParentId != null)
            {
                _logger.Log(LogLevel.Trace, $"Getting parents of product with id='{id}'...");
                productsIds.Add((int)product.ParentId);
                if (product.ParentProduct.ParentId != null)
                    productsIds.Add((int)product.ParentProduct.ParentId);

                _logger.Log(LogLevel.Trace, $"Parents of product with id='{id}' successfully received");
            }

            // Выполняем отдельный LoadAsync ибо долго грузит если инклюдить
            // TODO: Почему, если добавить сюда 
            // .GroupBy(a => new { a.AttributeId, a.ProductId }).Select(g => g.OrderByDescending(av => av.CreateTime).First()), то приходит пустота, 
            // но если вместо Load стоит ToList, то всё норм
            _logger.Log(LogLevel.Trace, $"Getting attribute values of products('{String.Join(',', productsIds)}')...");
            await _context.AttributeValues.Where(av => productsIds.Contains(av.ProductId)).LoadAsync();
            _logger.Log(LogLevel.Trace, $"Attribute values of products successfully received");

            _logger.Log(LogLevel.Trace, $"Getting product files of products('{String.Join(',', productsIds)}')...");
            await _context.ProductFiles.Where(av => productsIds.Contains(av.ProductId)).LoadAsync();
            _logger.Log(LogLevel.Trace, $"Product files of products successfully received");
            // -||-
            _logger.Log(LogLevel.Trace, $"Getting attribute permissions and attribute categories...");
            await _context.Attributes.Include(a => a.AttributePermissions).Include(a => a.AttributeCategories)
                .Where(a => a.DeleteTime == null && a.AttributeCategories.Any(ac => product.CategoryId == ac.CategoryId)).LoadAsync();
            _logger.Log(LogLevel.Trace, $"Attribute permissions and attribute categories successfully received");

            _logger.Log(LogLevel.Trace, $"Filtering attribute values...");
            // Анонимус для фильтрации нужных AttributeValues
            void Executive(Product pr)
            {
                pr.AttributeValues = pr.AttributeValues
                    .Where(
                        av => av.Attribute != null && user.UserRoles.Select(ur => ur.Role).Any(
                            r => av.Attribute.AttributePermissions
                                                .Select(ap => ap.RoleId)
                                                .Contains(r.Id)
                        )
                        && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == pr.CategoryId && ac.ModelLevel == pr.ModelLevel))
                    .GroupBy(a => new { a.AttributeId, a.ProductId })
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            }

            // Пробегаемся по всему дереву и фильтруем AttrbiuteValues
            DoForEachProductInTree(product, Executive);
            _logger.Log(LogLevel.Trace, $"Filtering attribute values is finished");

            _logger.Log(LogLevel.Debug, $"Getting product by id='{id}' is finished.");
            return Ok(_transformModelHelper.TransformProduct(product));
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetProductCmpyIds()
        {
            _logger.Log(LogLevel.Debug, "Start getting products by ids...");

            var ids = new List<int>();

            if (Request.Query.ContainsKey("ids") && !string.IsNullOrEmpty(Request.Query["ids"].ToString()))
            {
                _logger.Log(LogLevel.Trace, "Getting IDs of products from the request query...");
                ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"IDs of products '{String.Join(',', ids)}' from the request query successfully received");
            }

            _logger.Log(LogLevel.Trace, "Getting products...");
            var products = await _context.Products.Where(p => p.DeleteTime == null && ids.Contains(p.Id))
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts)
                .Include(p => p.AttributeValues)
                .ThenInclude(avl => avl.Attribute)
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Getting attribute values...");
            foreach (var product in products)
            {
                product.AttributeValues = product.AttributeValues.Where(av => av.Attribute.DeleteTime == null)
                    .GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            }
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Debug, "Getting products by ids is finished");
            return Ok(products.Select(p => _transformModelHelper.TransformProduct(p)).ToList());
        }

        [HttpGet("calculator")]
        public async Task<IActionResult> GetProductCmpyBrandAndSeason([FromQuery] int brandId, [FromQuery] int seasonId)
        {
            _logger.Log(LogLevel.Debug, $"Start getting products by brand='{brandId}' and season='{seasonId}'...");

            _logger.Log(LogLevel.Trace, $"Getting products by brand='{brandId}' and season='{seasonId}'...");
            var products = await _context.Products.Where(p => p.DeleteTime == null)
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts)
                .Include(p => p.ProductFiles)
                .Include(p => p.AttributeValues)
                .ThenInclude(avl => avl.Attribute)
                .Where(p => p.AttributeValues.Any(av => av.ListValueId == brandId) && p.AttributeValues.Any(av => av.ListValueId == seasonId))
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Getting attribute values...");
            foreach (var product in products)
            {
                product.AttributeValues = product.AttributeValues.Where(av => av.Attribute.DeleteTime == null)
                    .GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            }
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Debug, "Getting products by brand and season is finished.");
            return Ok(products.Select(p => _transformModelHelper.TransformProduct(p)).ToList());
        }

        [HttpPost("deals")]
        public async Task<IActionResult> GetProductCmpySku([FromBody] string[] skus)
        {
            _logger.Log(LogLevel.Debug, $"Start getting products by SKU '{String.Join(',', skus)}'...");

            _logger.Log(LogLevel.Trace, "Getting products...");
            var products = await _context.Products.Where(p => p.DeleteTime == null)
               .Where(p => skus.Contains(p.Sku))
               .Include(p => p.ParentProduct)
               .ThenInclude(p => p.ParentProduct)
               .ToListAsync();
            _logger.Trace("Including data to products is finished");

            _logger.Trace("Query async loading is finished");

            // Достаем все Idшники продуктов
            var productsIds = new List<int>();
            var productCategoriesIds = new List<int>();

            _logger.Trace("Getting products models...");
            var models = _context.Products.Local.Select(p => p.ParentProduct?.ParentProduct).Where(p => p != null).Distinct();
            _logger.Trace("Products models successfully received");

            _logger.Trace("Filling properties of products models...");
            foreach (var product in models)
            {
                try
                {
                    productsIds.AddRange(GetPropertiesFromProductTree(product, p => p.Id));
                    productCategoriesIds.AddRange(GetPropertiesFromProductTree(product, p => p.CategoryId.Value).Distinct());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            _logger.Trace("Properties of products models are filled");
            // Выполняем отдельный LoadAsync ибо долго грузит если инклюдить
            _logger.Trace("Async loading attribute values...");
            await _context.AttributeValues
                .Where(av => productsIds.Contains(av.ProductId))
                .LoadAsync();
            _logger.Trace("Async loading attribute values is finished");

            _logger.Trace("Async loading attributes...");
            await _context.Attributes.Include(a => a.AttributePermissions).Include(a => a.AttributeCategories)
                .Where(a => a.DeleteTime == null && a.AttributeCategories.Any(ac => productCategoriesIds.Contains(ac.CategoryId))).LoadAsync();
            _logger.Trace("Async loading attributes is finished");

            _logger.Trace("Filtering attribute values...");
            void Executive(Product product)
            {
                product.AttributeValues = product.AttributeValues.Where(
                    av =>
                        av.Attribute != null
                        && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId && ac.ModelLevel == product.ModelLevel)
                )
                .GroupBy(a => new { a.AttributeId, a.ProductId })
                .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                .ToList();
            };

            foreach (var model in models)
            {
                DoForEachProductInTree(model, Executive);
            }
            _logger.Trace($"Filtering attribute values for models is finished");

            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Combining attribute values in product...");
            foreach (var product in products)
                product.AttributeValues = GetAllAttributeValues(product);
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Debug, "Getting products by SKU is finished.");
            return Ok(products.Select(p => _transformModelHelper.TransformProduct(p)).ToList());
        }

        [HttpPost("ids")]
        public async Task<IActionResult> GetProductCmpyIdsPost([FromBody] int[] ids)
        {
            _logger.Log(LogLevel.Debug, $"Start getting products by IDs '{String.Join(',', ids)}'...");

            _logger.Log(LogLevel.Trace, "Getting products...");
            var products = await _context.Products.Where(p => p.DeleteTime == null)
               .Where(p => ids.Contains(p.Id))
               .Include(p => p.ParentProduct)
               .ThenInclude(p => p.ParentProduct)
               .ToListAsync();
            _logger.Trace("Including data to products is finished");

            _logger.Trace("Query async loading is finished");

            // Достаем все Idшники продуктов
            var productsIds = new List<int>();
            var productCategoriesIds = new List<int>();

            _logger.Trace("Getting products models...");
            var models = _context.Products.Local.Select(p => p.ParentProduct?.ParentProduct).Where(p => p != null).Distinct();
            _logger.Trace("Products models successfully received");

            _logger.Trace("Filling properties of products models...");
            foreach (var product in models)
            {
                try
                {
                    productsIds.AddRange(GetPropertiesFromProductTree(product, p => p.Id));
                    productCategoriesIds.AddRange(GetPropertiesFromProductTree(product, p => p.CategoryId.Value).Distinct());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            _logger.Trace("Properties of products models are filled");
            // Выполняем отдельный LoadAsync ибо долго грузит если инклюдить
            _logger.Trace("Async loading attribute values...");
            await _context.AttributeValues
                .Where(av => productsIds.Contains(av.ProductId))
                .LoadAsync();
            _logger.Trace("Async loading attribute values is finished");

            _logger.Trace("Async loading attributes...");
            await _context.Attributes.Include(a => a.AttributePermissions).Include(a => a.AttributeCategories)
                .Where(a => a.DeleteTime == null && a.AttributeCategories.Any(ac => productCategoriesIds.Contains(ac.CategoryId))).LoadAsync();
            _logger.Trace("Async loading attributes is finished");

            _logger.Trace("Filtering attribute values...");
            void Executive(Product product)
            {
                product.AttributeValues = product.AttributeValues.Where(
                    av =>
                        av.Attribute != null
                        && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId && ac.ModelLevel == product.ModelLevel)
                )
                .GroupBy(a => new { a.AttributeId, a.ProductId })
                .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                .ToList();
            };

            foreach (var model in models)
            {
                DoForEachProductInTree(model, Executive);
            }
            _logger.Trace($"Filtering attribute values for models is finished");

            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Combining attribute values in product...");
            foreach (var product in products)
                product.AttributeValues = GetAllAttributeValues(product);
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Debug, "Getting products by Ids is finished.");
            return Ok(products.Select(p => _transformModelHelper.TransformProduct(p)).ToList());
        }

        [HttpGet("update-search")]
        public async Task<IActionResult> UpdateSearchStringInProducts()
        {
            _logger.Log(LogLevel.Debug, "Start updating search strings...");

            var skip = 0;
            var take = 5;
            var i = 0;
            _logger.Log(LogLevel.Trace, "Getting list values...");
            var listValues = await _context.ListValues.Where(lv => lv.DeleteTime == null).ToListAsync();
            _logger.Log(LogLevel.Trace, "List values successfully received");

            _logger.Log(LogLevel.Trace, "Setting categoriesIds...");
            var categoriesIds = new List<int> { 3, 31, 941, 227, 942, 228, 229, 230, 231, 931, 873, 874, 875, 876, 924, 42, 168, 169, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 182, 916, 43, 181, 209, 211, 212, 239, 240, 262, 241, 44, 242, 243, 245, 244, 246, 45, 247, 248, 46, 249, 251, 47, 252, 254, 255, 256, 48, 257, 258, 529, 546, 549, 550, 49, 214, 215, 496, 494, 495, 497, 498, 507, 509, 526, 236, 237, 238, 259, 260, 261, 223, 499, 500, 501, 502, 930, 936, 213, 50, 943, 889, 923, 944, 216, 948, 945, 217, 218, 949, 946, 219, 220, 226, 221, 222, 225, 235, 224, 950, 947, 921, 951, 888, 891, 892, 911, 912, 913, 914, 915, 917, 918, 919 };
            _logger.Log(LogLevel.Trace, "CategoriesIds are set");

            _logger.Log(LogLevel.Trace, "Updating search strings...");
            while (true)
            {
                var products = await _context.Products
                    .Where(p => p.DeleteTime == null
                        && categoriesIds.Contains(p.CategoryId.Value)
                        && p.ModelLevel == ModelLevel.Model
                        && p.CreateTime > DateTime.Now.AddDays(-10)
                        && p.ProductSearch == null)
                    .Include(p => p.SubProducts)
                    .ThenInclude(sp => sp.SubProducts)
                    .OrderByDescending(p => p.Id)
                    //.Skip(skip)
                    .Take(take)
                    .ToListAsync();

                if (products.Count == 0)
                    break;

                // Getting all products Ids
                var productsIds = new List<int>();
                foreach (var product in products)
                {
                    try
                    {
                        productsIds.AddRange(GetPropertiesFromProductTree(product, p => p.Id));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                // Include AttributeValues and Categories
                await _context.AttributeValues
                    .Include(av => av.Attribute)
                    .ThenInclude(a => a.AttributeCategories)
                    .ThenInclude(ac => ac.Category)
                    .Where(av => av.Attribute.DeleteTime == null && productsIds.Contains(av.ProductId))
                    .GroupBy(a => new { a.ProductId, a.AttributeId })
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .LoadAsync();

                // Including ProductSearch
                await _context.ProductSearch.Where(ps => productsIds.Contains(ps.ProductId)).LoadAsync();

                foreach (var product in products)
                {
                    DoForEachProductInTree(product,
                        delegate (Product pr)
                        {
                            pr.UpdateSearchStringArray();
                            Console.WriteLine($"Added Search string for {pr.Sku}. Model level: {pr.ModelLevel}. Parent SKU: {pr.ParentProduct?.Sku} -> {pr.ParentProduct?.ParentProduct?.Sku}");
                        });
                }

                skip += take;
                i += take;

                Console.Write($"{i} models were updated.");

                await _context.SaveChangesAsync();
            }
            _logger.Log(LogLevel.Trace, "Search strings are updated.");

            _logger.Log(LogLevel.Debug, "Updating search strings is finished.");
            return Ok();
        }

        // Code for manual updating of search strings
        [HttpPut("update-search")]
        public async Task<IActionResult> UpdateSearchString()
        {
            _logger.Log(LogLevel.Debug, "Start updating search strings...");
            var skip = 0;
            var take = 1000;

            _logger.Log(LogLevel.Trace, "Getting attribute values...");
            var attrValues = await _context.AttributeValues.Where(av => av.SearchVector == null)
                                                           .Include(a => a.Attribute)
                                                           .Where(av => av.Attribute.DeleteTime == null)
                                                           .GroupBy(g => g.ProductId)
                                                           .Select(t => t.GroupBy(s => s.AttributeId)
                                                            .Select(d => d.OrderByDescending(h => h.CreateTime).First())
                                                           )
                                                           .Skip(skip)
                                                           .Take(take)
                                                           .ToListAsync();
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Trace, "Updating search strings...");
            while (attrValues.Count > 0)
            {
                var listValues = await _context.ListValues.Where(lv => lv.DeleteTime == null).ToListAsync();
                int i = 0;//для просмотра очереди

                foreach (var attr in attrValues)
                {
                    foreach (var a in attr)
                    {
                        switch (a.Attribute.Type)
                        {
                            case AttributeType.Boolean:
                                a.SearchString = (a.BoolValue != null ? ((bool)a.BoolValue ? "ДА" : "НЕТ") : null);
                                break;
                            case AttributeType.List:
                                a.SearchString = (listValues.Where(lv => lv.DeleteTime == null && lv.Id == a.ListValueId)?.FirstOrDefault()?.Value).Trim().ToUpper();
                                break;
                            case AttributeType.Number:
                                a.SearchString = a.NumValue?.ToString().Trim().ToUpper();
                                break;
                            case AttributeType.String:
                                a.SearchString = a.StrValue.Trim().ToUpper();
                                break;
                            case AttributeType.Text:
                                a.SearchString = a.StrValue.Trim().ToUpper();
                                break;
                            case AttributeType.Date:
                                a.SearchString = a.DateValue?.ToString().Trim().ToUpper();
                                break;
                            default:
                                throw new ArgumentException("Wrong type of attribute.");
                        }

                        //a.SearchString = (a.StrValue + " " + a.NumValue?.ToString() + " " + (listValues.Where(lv => lv.DeleteTime == null && lv.Id == a.ListValueId)?.FirstOrDefault()?.Value) + " " + (a.BoolValue != null ? ((bool)a.BoolValue ? "Да" : "Нет") : null) + " " + a.DateValue?.ToString()).Trim().ToLower();
                    }

                    i++;
                }

                await _context.SaveChangesAsync();

                skip += take;
                attrValues = await _context.AttributeValues.Where(av => av.SearchVector == null).Include(a => a.Attribute)
                                                                       .Where(av => av.Attribute.DeleteTime == null)
                                                                       .GroupBy(g => g.ProductId)
                                                                       .Select(t => t.GroupBy(s => s.AttributeId)
                                                                       .Select(d => d.OrderByDescending(h => h.CreateTime).First()))
                                                                       .Skip(skip)
                                                                       .Take(take)
                                                                       .ToListAsync();
            }

            _logger.Log(LogLevel.Trace, "Search strings are updated.");

            _logger.Log(LogLevel.Debug, "Updating search strings is finished.");
            return Ok();
        }
   
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] int? pageSize, [FromQuery] int? pageNumber, [FromQuery] string sortByPropertyName,
                                                        [FromQuery] bool withoutCategory, [FromQuery] bool sortByAsc = true, [FromQuery] string ageMonthRange = "-",
                                                        [FromQuery] string ageYearRange = "-", [FromQuery] string priceRange = "-", [FromQuery] string currency = "EUR")
        {
            _logger.Log(LogLevel.Debug, "Start searching products...");

            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _webApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            var coefExchange = 1.0f;

            try
            {
                if (currency != "EUR")
                    coefExchange = await GetCurrentExchangeCoefficientAsync(currency);
            }
            catch (Exception e)
            {
                _logger.Error($"There are no current policy with seasonId: {_currentSeasonOptions.Id}");
            }

            _logger.Log(LogLevel.Trace, "Getting categories from the request query...");
            var categories = Request.Query["categories"].Count == 0
                                       ? new List<int>()
                                       : Request.Query["categories"].ToString()
                                                                    .Split(',')
                                                                    .Select(int.Parse)
                                                                    .ToList();
            _logger.Log(LogLevel.Trace, $"Categories '{String.Join(',', categories)}' from the request query successfully received");

            _logger.Log(LogLevel.Trace, "Getting attributesIds from the request query...");
            var attributesIds = Request.Query["attributesIds"].Count == 0
                           ? new List<int>()
                           : Request.Query["attributesIds"].ToString()
                                                        .Split(',')
                                                        .Select(int.Parse)
                                                        .ToList();
            _logger.Log(LogLevel.Trace, $"AttributesIds '{String.Join(',', attributesIds)}' from the request query successfully received");

            _logger.Log(LogLevel.Trace, "Getting the search string...");
            var searchStr = JsonConvert.DeserializeObject<string[]>(Request.Query["searchString"].ToString());
            _logger.Log(LogLevel.Trace, "Search string successfully received");

            _logger.Log(LogLevel.Trace, "Creating search struct...");
            Search search = FillingSearchStruct(searchStr);
            _logger.Log(LogLevel.Trace, "Search struct is created");
            _logger.Log(LogLevel.Trace, "Getting products...");
            var products = await GetProductsPagedResultAsync(search, user, attributesIds, pageSize, pageNumber, categories, withoutCategory,
                sortByPropertyName, sortByAsc, ageMonthRange, ageYearRange, priceRange, coefExchange);
            _logger.Log(LogLevel.Trace, "Products successfully received");

            try
            {
                _logger.Log(LogLevel.Trace, "Formation the result response...");
                Request.HttpContext.Response.Headers.Add("X-Total-Count", products.RowCount.ToString());
                Request.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "X-Total-Count");
                _logger.Log(LogLevel.Trace, "The result response is formed");

                _logger.Log(LogLevel.Debug, "Searching products is finished");
                return Ok(products);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get products. Error: {e.Message}");
                return BadRequest(e.Message);
                //throw new Exception(e.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            _logger.Log(LogLevel.Debug, "Start creating product...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }
            _logger.Info("Start creating product...");
            _logger.Log(LogLevel.Trace, "Getting user ID...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
            _logger.Log(LogLevel.Trace, $"User ID='{userId}' successfully received");

            _logger.Trace("Getting listValueId from product tree...");
            var productListValuesIds = GetPropertiesFromProductTree(product, p => p.AttributeValues.Select(av => av.ListValueId).Where(lvi => lvi != null).ToList())
                .SelectMany(av => av).Distinct();
            _logger.Trace($"ListValueId('{String.Join(',', productListValuesIds)}') from product tree successfully received");

            _logger.Trace("Getting list values...");
            var listValues = await _context.ListValues.Where(lv => lv.DeleteTime == null && productListValuesIds.Contains(lv.Id)).ToListAsync();
            _logger.Log(LogLevel.Trace, "List values successfully received");

            _logger.Trace("Getting last product...");
            var lastProduct = _context.Products.LastOrDefault();
            _logger.Log(LogLevel.Trace, $"last product with id='{lastProduct.Id}' successfully received");

            var lastDbId = lastProduct?.Id ?? 0;

            // Подгружаем атрибуты
            _logger.Trace("Getting attributes...");
            var createdProducts = GetPropertiesFromProductTree(product, p => p);
            var createdProductsAttributesIds = createdProducts.SelectMany(ep => ep.AttributeValues).Where(av => av.Id == 0).Select(av => av.AttributeId);
            await _context.Attributes
                .Where(a => createdProductsAttributesIds.Contains(a.Id))
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "Attributes successfully received");

            _logger.Trace("Filling product tree");
            FillProductTree(userId, product.CategoryId, ref lastDbId, listValues, new List<Product> { product }, ModelLevel.Model);
            _logger.Log(LogLevel.Trace, "Product tree is filled");
            try
            {
                createdProducts.ForEach(cp => cp.AttributeValues.ForEach(av => av.Attribute = null));

                _context.Products.Add(product);
                _logger.Trace($"Saving product with id='{product.Id}' to database");
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Error($"Couldn't save product. Error: {e.Message}");
                return BadRequest(e);
            }
            _logger.Info("Product was successfully saved");
            var attrValues = new List<AttributeValue>();

            _logger.Log(LogLevel.Trace, "Formation the result response...");
            GetPropertiesFromProductTree(product, p => p.AttributeValues).ForEach(avl => attrValues.AddRange(avl));
            var attrIdsWithNullAttributes = attrValues.Where(av => av.Attribute == null).Select(av => av.AttributeId);
            var attributes = await _context.Attributes.Where(a => attrIdsWithNullAttributes.Contains(a.Id)).ToDictionaryAsync(av => av.Id);
            attrValues.Where(av => av.Attribute == null).ToList().ForEach(av => av.Attribute = attributes[av.AttributeId]);
            _logger.Log(LogLevel.Trace, "The result response is formed");

            _logger.Log(LogLevel.Debug, "Creating product is finished.");
            return Ok(_transformModelHelper.TransformProduct(product));
        }



        [HttpPut("properties")]
        public async Task<IActionResult> CreateAttributeValues([FromBody] List<AttributeValue> attributeValues)
        {
            // TODO: Добавить Update поисковой строки у продуктов
            _logger.Log(LogLevel.Info, "Start creating attribute values...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
            User user = null;
            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = await _webApiCommunicator.GetUser(userId);
                _logger.Log(LogLevel.Trace, "Deserializing response data ...");
                user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, "Getting attribute permissions by user roles...");
            var attributeIdCmpyPermissions = _context.AttributePermissions.Where(ap => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id).Contains(ap.RoleId) && ap.Value > DataAccessMethods.Read).Select(ap => ap.AttributeId).ToList();
            _logger.Log(LogLevel.Trace, "Attribute permissions successfully received");

            _logger.Log(LogLevel.Trace, "Checking attribute values...");
            if (!attributeValues.Select(p => p.AttributeId).All(attrId => attributeIdCmpyPermissions.Contains(attrId)))
            {
                return Forbid();
            }
            _logger.Log(LogLevel.Trace, "Attribute values are checked");

            _logger.Trace("Getting list values...");
            var listValues = await _context.ListValues.Where(lv => lv.DeleteTime == null && attributeValues.Select(p => p.ListValueId).Where(lvi => lvi != null).Contains(lv.Id)).ToListAsync();
            _logger.Log(LogLevel.Trace, "List values successfully received");


            var productsIds = attributeValues.Select(av => av.ProductId).Distinct();
            var attributesIds = attributeValues.Select(av => av.AttributeId).Distinct();

            _logger.Log(LogLevel.Trace, "Getting products by attribute values productId...");
            var products = await _context.Products.Where(p => productsIds.Contains(p.Id)).Include(p => p.ProductSearch).ToDictionaryAsync(p => p.Id);
            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Getting ids of categories...");
            var categoriesIds = products.Values.Select(p => p.CategoryId).Distinct();
            _logger.Log(LogLevel.Trace, "ids of categories successfully received");

            var attributeCategories = await _context.AttributeCategories
                .Where(ac => attributesIds.Contains(ac.AttributeId) && categoriesIds.Contains(ac.CategoryId))
                .Include(ac => ac.Attribute)
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "Filtering attribute values by category with same level...");

            var filterAttributeValueCmpyLevelAndCategory = attributeValues.Where(av =>
                                                        attributeCategories.Any(ac => ac.ModelLevel == products[av.ProductId].ModelLevel &&
                                                                                      ac.CategoryId == products[av.ProductId].CategoryId &&
                                                                                      ac.AttributeId == av.AttributeId)).ToList();

            _logger.Log(LogLevel.Trace, "Getting attributes by ids...");
            var filteredAttributesIds = filterAttributeValueCmpyLevelAndCategory.Select(av => av.AttributeId);
            var attrDictionary = await _context.Attributes
                .Where(a => filteredAttributesIds.Contains(a.Id))
                .AsNoTracking()
                .ToDictionaryAsync(a => a.Id);
            _logger.Log(LogLevel.Trace, "Attributes successfully got");

            _logger.Log(LogLevel.Trace, "Adding creator and create time to attribute values...");
            filterAttributeValueCmpyLevelAndCategory.ForEach(at =>
            {
                at.CreateTime = DateTime.Now;
                at.CreatorId = userId;
                at.Attribute = attrDictionary.ContainsKey(at.AttributeId) ? attrDictionary[at.AttributeId] : null;
                at.SearchString = AttributeValuesHelper.CreateSearchString(at, listValues.FirstOrDefault(lv => lv.Id == at.ListValueId)?.Value);
                at.Id = 0;
            });
            _logger.Log(LogLevel.Trace, "Creator and create time are added to attribute values");

            _logger.Log(LogLevel.Trace, "Updating search string by one attribute...");
            foreach (var product in products.Where(p => filterAttributeValueCmpyLevelAndCategory.Select(av => av.ProductId).Contains(p.Key)).Select(p => p.Value))
            {
                product.UpdateSearchStringByOneAttribute(filterAttributeValueCmpyLevelAndCategory.First(av => av.ProductId == product.Id));
            }
            _logger.Log(LogLevel.Trace, "Search string successfully updated");

            _logger.Log(LogLevel.Trace, "Filtring attribute values by level and category...");

            await _context.AttributeValues.AddRangeAsync(filterAttributeValueCmpyLevelAndCategory);
            _logger.Log(LogLevel.Trace, "Filtring attribute values by level and category is finished");
            try
            {
                _logger.Log(LogLevel.Trace, "Saving attribute values changes to database...");
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error to save attribute values. Error: {e.Message}");
                return BadRequest(e.Message);
            }

            _logger.Log(LogLevel.Info, "Creating attribute values is finished");
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Product editProduct)
        {
            _logger.Log(LogLevel.Info, $"Start editing product with id='{id}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            _logger.Info("Start editing product...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
            User user = null;
            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = await _webApiCommunicator.GetUser(userId);
                _logger.Log(LogLevel.Trace, "Deserializing response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Trace("Getting user permissions");
            var userRolesIds = user.UserRoles.Select(ur => ur.Role).Select(r => r.Id);
            var attributeIdCmpyPermissions = await _context.AttributePermissions.Where(ap => userRolesIds.Contains(ap.RoleId)).ToListAsync();
            _logger.Log(LogLevel.Trace, "User permissions successfully received");

            _logger.Trace($"Getting product with id='{id}'...");
            var dbProduct = await _context.Products
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts)
                .Include(p => p.Category.AttributeCategories)
                .Where(p => p.DeleteTime == null)
                .SingleOrDefaultAsync(c => c.Id == id);

            if (dbProduct == null)
            {
                _logger.Log(LogLevel.Error, $"Product with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Product with id='{id}' successfully received");

            _logger.Trace("Getting properties from product tree...");
            var dbProducts = GetPropertiesFromProductTree(dbProduct, p => p);
            var editProducts = GetPropertiesFromProductTree(editProduct, p => p);
            _logger.Log(LogLevel.Trace, "Properties from product tree successfully received");

            var dbProductsIds = dbProducts.Select(dp => dp.Id);

            _logger.Log(LogLevel.Trace, "Loading attribute values...");
            await _context.AttributeValues
                .Where(av => dbProductsIds.Contains(av.ProductId))
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "Attribute values successfully loaded");

            _logger.Log(LogLevel.Trace, "Loading attributes...");
            var dbProductAttributesIds = dbProducts.SelectMany(p => p.AttributeValues).Select(av => av.AttributeId).Distinct();
            var editProductsAttributesIds = editProducts.SelectMany(ep => ep.AttributeValues).Where(av => av.Id == 0).Select(av => av.AttributeId).Distinct();
            var attributesIds = dbProductAttributesIds.Union(editProductsAttributesIds).Distinct();

            await _context.Attributes
                  .Include(a => a.AttributePermissions)
                  .Include(a => a.AttributeCategories)
                  .Where(a => attributesIds.Contains(a.Id))
                  .LoadAsync();
            _logger.Log(LogLevel.Trace, "Attributes successfully loaded");

            _logger.Log(LogLevel.Trace, $"Loading product files of products('{String.Join(',', dbProductsIds)}')...");
            await _context.ProductFiles
                .Where(av => dbProductsIds.Contains(av.ProductId))
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "Product files successfully loaded");

            _logger.Log(LogLevel.Trace, $"Loading product search of products('{String.Join(',', dbProductsIds)}')...");
            await _context.ProductSearch
                .Where(ps => dbProductsIds.Contains(ps.ProductId))
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "Product search successfully loaded");

            _logger.Log(LogLevel.Trace, "Filtring attribute values...");
            dbProducts.ForEach(dp =>
            {
                dp.AttributeValues = dp.AttributeValues.Where(av => av.Attribute != null &&
                                                                    user.UserRoles.Select(ur => ur.Role).Any(r => av.Attribute.AttributePermissions.Select(ap => ap.RoleId).Contains(r.Id)) &&
                                                                    av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == dp.CategoryId && ac.ModelLevel == dp.ModelLevel))

                    .GroupBy(a => new { a.AttributeId, a.ProductId })
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            });
            _logger.Log(LogLevel.Trace, "Attribute values are filtered");

            try
            {
                _logger.Log(LogLevel.Trace, "Editing products trees...");
                await EditProductsTrees(dbProduct, editProduct, attributeIdCmpyPermissions, userId);
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error during editing products trees: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace("Product was edited");

            try
            {
                //Отключаем измененные атрибуты
                var changedAttr = _context.ChangeTracker.Entries<Common.Models.Pim.Attribute>().Where(e => e.State != EntityState.Unchanged).ToList();
                foreach (var entityEntry in changedAttr)
                {
                    entityEntry.State = EntityState.Detached;
                }

                _logger.Trace("Saving product to database...");
                await _context.SaveChangesAsync();
                _logger.Trace("Product was successfully saved");

                _logger.Trace("Getting attribute values...");
                var attrValues = GetPropertiesFromProductTree(dbProduct, p => p.AttributeValues).SelectMany(av => av).ToList();

                var attrIdsWithNullAttributes = attrValues.Where(av => av.Attribute == null).Select(av => av.AttributeId);
                var attributes = await _context.Attributes.Where(a => attrIdsWithNullAttributes.Contains(a.Id)).ToDictionaryAsync(av => av.Id);

                attrValues.Where(av => av.Attribute == null).ToList().ForEach(av => av.Attribute = attributes[av.AttributeId]);
                _logger.Log(LogLevel.Trace, "Attribute values successfully received");
                _logger.Log(LogLevel.Info, "Editing product is finished");

                return Ok(_transformModelHelper.TransformProduct(dbProduct));
            }
            catch (DbUpdateConcurrencyException e)
            {

                _logger.Error($"Couldn't save product. Error: {e.Message}");

                if (_context.Products.Any(c => c.Id == id))
                {
                    _logger.Log(LogLevel.Error, $"Product with id='{id}' not found");
                    return NotFound();
                }
                throw;
            }
        }

        private void CompareAndEdit(Product dbProduct, Product editProduct,
            List<AttributePermission> attributeIdCmpyPermissions,
            Dictionary<int, AttributeValue> attrValuesDictionary,
            Dictionary<int, ListValue> listValuesDictionary,
            int userId, ref int lastDbId, ModelLevel modelLevel)
        {
            _logger.Log(LogLevel.Info, "Start comparing and editing...");

            _logger.Log(LogLevel.Trace, "Editing product...");
            EditProduct(dbProduct, editProduct, attributeIdCmpyPermissions, attrValuesDictionary, listValuesDictionary, userId);
            _logger.Log(LogLevel.Trace, "Editing product  is finished");

            if (dbProduct.SubProducts == null)
            {
                _logger.Log(LogLevel.Error, "Sub products not found.");
                return;
            }

            _logger.Log(LogLevel.Trace, "Removing products...");
            foreach (var item in dbProduct.SubProducts.Where(dp => !editProduct.SubProducts.Select(ep => ep.Id).Contains(dp.Id)).ToList())
            {
                RemoveProducts(new List<Product>() { item }, userId);
            }
            _logger.Log(LogLevel.Trace, "Removing products is finished");

            _logger.Log(LogLevel.Trace, "Updating subproducts...");
            var dbProductsIds = dbProduct.SubProducts.Where(dp => dp.Id != 0).Select(dp => dp.Id).ToList();
            foreach (var item in editProduct.SubProducts.Where(p => p.DeleteTime == null).ToList())
            {
                if (!dbProductsIds.Contains(item.Id))
                {

                    item.CreateTime = DateTime.Now;
                    item.CreatorId = userId;
                    item.Sku = _skuGenerator.GenerateSku(lastDbId);
                    item.ModelLevel = modelLevel;
                    lastDbId++;
                    if (dbProduct.Id != 0)
                    {
                        dbProduct.SubProducts.Add(item);
                    }
                    CompareAndEdit(item, item, attributeIdCmpyPermissions, attrValuesDictionary, listValuesDictionary, userId, ref lastDbId, modelLevel + 1);
                }
                else
                {
                    CompareAndEdit(dbProduct.SubProducts.First(p => p.Id == item.Id), item, attributeIdCmpyPermissions, attrValuesDictionary, listValuesDictionary, userId, ref lastDbId, modelLevel + 1);
                }

            }
            _logger.Log(LogLevel.Trace, "Updating subproducts is finished");

            _logger.Log(LogLevel.Info, "Comparing and editing is finished");
        }

        private async Task EditProductsTrees(Product dbProduct, Product editProduct, List<AttributePermission> attributeIdCmpyPermissions, int userId)
        {
            _logger.Log(LogLevel.Info, "Start editing products trees...");

            _logger.Log(LogLevel.Trace, "Getting attrValues from dbProduct and editProduct...");
            var editProductAttrValues = GetPropertiesFromProductTree(editProduct, p => p.AttributeValues).SelectMany(av => av).ToList();
            var dbProductsAttrValues = GetPropertiesFromProductTree(dbProduct, p => p.AttributeValues).SelectMany(av => av).ToList();
            _logger.Log(LogLevel.Trace, " attrValues from dbProduct and editProduct successfully received");

            _logger.Trace("Getting product list values ids...");
            var listValuesIds = editProductAttrValues.Where(av => av.ListValueId != null)
                .Select(av => av.ListValueId.Value)
                .Distinct().ToList();
            _logger.Log(LogLevel.Trace, "Product list values ids successfully received");

            _logger.Trace($"Getting list values of products('{String.Join(',', listValuesIds)}')...");
            var listValuesDictionary = await _context.ListValues.Where(lv => lv.DeleteTime == null && listValuesIds.Contains(lv.Id)).ToDictionaryAsync(lv => lv.Id);
            _logger.Log(LogLevel.Trace, "List values successfully received");

            _logger.Log(LogLevel.Info, "Creating attrValues dictionary...");
            var attrValuesDictionary = new Dictionary<int, AttributeValue>();
            foreach (var av in dbProductsAttrValues)
            {
                if (!attrValuesDictionary.ContainsKey(av.Id))
                    attrValuesDictionary.Add(av.Id, av);
            }
            _logger.Log(LogLevel.Info, "AttrValues dictionary successfully created");

            _logger.Trace("Getting last product...");
            var lastProduct = _context.Products.OrderBy(p => p.Id).LastOrDefault();
            _logger.Log(LogLevel.Trace, $"Last product with id='{lastProduct.Id}' successfully received");

            var lastDbId = lastProduct?.Id ?? 0;
            _logger.Trace("Starting compare product with received product...");
            CompareAndEdit(dbProduct, editProduct, attributeIdCmpyPermissions, attrValuesDictionary, listValuesDictionary, userId, ref lastDbId, ModelLevel.ColorModel);
            _logger.Trace("Comparing product with received product is finished");

            _logger.Log(LogLevel.Info, "Editing products trees is finished");
        }


        private void EditProduct(
            Product dbProduct,
            Product editProduct,
            List<AttributePermission> attributeIdCmpyPermissions,
            Dictionary<int, AttributeValue> attrValuesDictionary,
            Dictionary<int, ListValue> listValuesDictionary,
            int userId)
        {
            _logger.Log(LogLevel.Info, "Start editing product...");

            _logger.Trace("Updating product fields...");
            dbProduct.Name = editProduct.Name;
            dbProduct.CategoryId = editProduct.CategoryId;
            dbProduct.ModelLevel = editProduct.ModelLevel;
            _logger.Trace("Product fields are updated");

            _logger.Trace("Checking permissions and editing attributeValues...");
            foreach (var attributeValue in editProduct.AttributeValues.ToList())
            {
                try
                {
                    var origAttrValue = attributeValue.Id != 0 ? attrValuesDictionary[attributeValue.Id] : attributeValue;
                    if (attributeValue.Id <= 0 || AttributeValueWasChanged(origAttrValue, attributeValue))
                    {
                        if (!attributeIdCmpyPermissions.Any(ap => ap.AttributeId == attributeValue.AttributeId && ap.Value > DataAccessMethods.Read))
                        {
                            _logger.Error($"User doesn't have permissions to edit attributeValue with Id={attributeValue.AttributeId}");
                            throw new Exception("User doesn't have permissions to edit attributeValue");
                        }

                        var listValue = attributeValue.ListValueId.HasValue ? listValuesDictionary[attributeValue.ListValueId.Value].Value : null;

                        attributeValue.SearchString = AttributeValuesHelper.CreateSearchString(attributeValue, listValue);
                        attributeValue.CreateTime = DateTime.Now;
                        attributeValue.CreatorId = userId;
                        attributeValue.Id = 0;
                        //attributeValue.Attribute = null;
                        dbProduct.AttributeValues.Add(attributeValue);
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Error on editing attributeValue");
                    throw;
                }
            }
            _logger.Trace("Permissions and editing attribute values are checked");
            dbProduct.UpdateSearchStringArray();

            _logger.Trace("Adding productFiles to product");
            var productFilesIds = editProduct.ProductFiles.Where(pf => pf.FileType == FileType.Document || pf.FileType == FileType.Image || pf.FileType == FileType.Video).ToDictionary(pf => pf.FileId);
            dbProduct.ProductFiles.RemoveAll(pf => !productFilesIds.Keys.Contains(pf.FileId));

            foreach (var pfId in productFilesIds.Keys)
            {
                if (!dbProduct.ProductFiles.Select(pf => pf.FileId).Contains(pfId))
                    dbProduct.ProductFiles.Add(
                        new ProductFile()
                        {
                            FileId = pfId,
                            ProductId = dbProduct.Id,
                            FileType = productFilesIds[pfId].FileType,
                            IsMain = productFilesIds[pfId].IsMain
                        });
            }
            _logger.Trace("Setting product mainImage");
            var mainImgId = editProduct.ProductFiles.FirstOrDefault(pf => pf.IsMain)?.FileId;
            dbProduct.ProductFiles.ForEach(pf => pf.IsMain = pf.FileId == mainImgId);
            _logger.Trace("Product files are added to product");

            _logger.Log(LogLevel.Info, "Editing product is finished");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.Log(LogLevel.Info, "Start deleting product...");

            _logger.Trace($"Getting product by id='{id}'...");
            var product = await _context.Products
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (product == null)
            {
                _logger.Log(LogLevel.Error, $"Product with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Product with id='{id}' successfully received");

            _logger.Trace("Getting userId...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
            _logger.Log(LogLevel.Trace, $"UserId='{userId}' successfully received");

            _logger.Trace($"Removing product with id='{product.Id}'...");
            RemoveProducts(new List<Product>() { product }, userId);
            _logger.Trace($"Removing product with id='{product.Id}' is finished");

            _logger.Trace($"Updating state on '{EntityState.Modified}'...");
            _context.Entry(product).State = EntityState.Modified;
            _logger.Trace($"State is updated");

            //await _context.SaveChangesAsync();
            try
            {
                _logger.Log(LogLevel.Trace, "Saving product changes to database...");
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error to save product values. Error: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace("Product was successfully saved");

            _logger.Log(LogLevel.Info, "Deleting is finished");
            return Ok(_transformModelHelper.TransformProduct(product));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteProducts()
        {
            _logger.Log(LogLevel.Info, "Start deleting products...");


            var ids = new List<int>();
            if (Request.Query.ContainsKey("ids") && !string.IsNullOrEmpty(Request.Query["ids"].ToString()))
            {
                _logger.Trace("Getting ids of products from query...");
                ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"Ids='{String.Join(',', ids)}' successfully received");
            }

            _logger.Trace("Getting userId...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
            _logger.Trace($"UserId='{userId}' successfully received");

            _logger.Trace("Getting products to remove...");
            var productsToRemove = await _context.Products
                .Include(p => p.SubProducts)
                .ThenInclude(p => p.SubProducts)
                .Where(p => ids.Contains(p.Id)).ToListAsync();
            _logger.Log(LogLevel.Trace, $"Products to remove successfully received");

            _logger.Trace("Removing products...");
            RemoveProducts(productsToRemove, userId);
            _logger.Trace("Products are removed");

            //await _context.SaveChangesAsync();
            try
            {
                _logger.Log(LogLevel.Trace, "Saving product changes to database...");
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error to save product values. Error: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace("Product was successfully saved");

            _logger.Log(LogLevel.Info, "Deleting products is finished");
            return Ok(productsToRemove.Select(p => _transformModelHelper.TransformProduct(p)));
        }

        private void RemoveProducts(ICollection<Product> products, int userId)
        {
            _logger.Log(LogLevel.Info, "Start removing products...");

            _logger.Log(LogLevel.Error, "Checking products for removing...");
            if (products == null || !products.Any())
            {
                _logger.Log(LogLevel.Error, "Products for removing not found.");
                return;
            }
            _logger.Log(LogLevel.Error, "Products for removing are checked");

            foreach (var product in products)
            {
                _logger.Log(LogLevel.Error, $"Updating fields of product with id='{product.Id}' ...");
                product.DeleteTime = DateTime.Now;
                product.DeleterId = userId;
                _logger.Log(LogLevel.Error, $"Fields of product with id='{product.Id}' are updated");
                RemoveProducts(product.SubProducts, userId);
            }

            _logger.Log(LogLevel.Info, "Removing products is finished");

        }

        private void FillProductTree(int userId, int? categoryId, ref int lastDbId, List<ListValue> listValues, ICollection<Product> products, ModelLevel modelLevel)
        {
            _logger.Log(LogLevel.Info, "Start filling product tree...");

            if (products != null && products.Any())
            {
                foreach (var product in products)
                {
                    _logger.Log(LogLevel.Error, $"Updating fields of product with id='{product.Id}' ...");
                    product.CreateTime = DateTime.Now;
                    product.CreatorId = userId;
                    product.Sku = _skuGenerator.GenerateSku(lastDbId);
                    product.CategoryId = categoryId;
                    product.ModelLevel = modelLevel;
                    product.AttributeValues.ForEach(at =>
                    {
                        at.SearchString = AttributeValuesHelper.CreateSearchString(at, listValues.FirstOrDefault(lv => lv.Id == at.ListValueId)?.Value);
                        at.CreateTime = DateTime.Now;
                        at.CreatorId = userId;
                        //at.Attribute = null;
                    });
                    lastDbId++;
                    //Та же ошибка что и в редактировании
                    product.UpdateSearchStringArray();

                    FillProductTree(userId, categoryId, ref lastDbId, listValues, product.SubProducts, modelLevel + 1);

                    _logger.Log(LogLevel.Error, $"Fields of product with id='{product.Id}' are updated");
                }
            }
            _logger.Log(LogLevel.Info, "Filling product tree is finished");
        }

        private bool AttributeValueWasChanged(AttributeValue av, AttributeValue avd)
        {
            switch (av.Attribute.Type)
            {
                case AttributeType.Boolean:
                    return av.BoolValue != avd.BoolValue;
                case AttributeType.List:
                    return av.ListValueId != avd.ListValueId;
                case AttributeType.Number:
                    return av.NumValue != avd.NumValue;
                case AttributeType.String:
                    return av.StrValue != avd.StrValue;
                case AttributeType.Text:
                    return av.StrValue != avd.StrValue;
                case AttributeType.Date:
                    return av.DateValue != avd.DateValue;
                default:
                    throw new ArgumentException("Wrong type of attribute.");
            }
        }

        private Search FillingSearchStruct(string[] searchStr)
        {
            _logger.Log(LogLevel.Info, "Start filling search struct...");
            Search search = new Search();
            try
            {
                if (searchStr == null)
                    return search;

                foreach (string str in searchStr)
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        var strTemp = str.Split(new char[] { ':' }, 2);
                        if (strTemp.Count() > 1 && !String.IsNullOrEmpty(strTemp[0]))
                        {
                            if (strTemp[0].ToUpper().Contains(SkuParametr))
                            {
                                search.sku = strTemp[1].Trim().ToUpper().Split(new string[] { "||" }, StringSplitOptions.None).ToList();
                            }
                            else if (strTemp[0].ToUpper().Contains(ImportsParametr))
                            {
                                var strArray = strTemp[1].Trim().Split(new string[] { "||" }, StringSplitOptions.None);
                                bool nullFlag = false;
                                string[] strImports = null;

                                if (strArray.Contains(""))
                                {
                                    nullFlag = true;
                                    strImports = strArray.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                                }

                                if (strImports != null)
                                {
                                    search.imports = strImports.Select(ch => ToNullableInt(ch)).ToList();
                                }
                                else
                                {
                                    search.imports = strArray.Select(ch => ToNullableInt(ch)).ToList();
                                }

                                if (nullFlag)
                                    search.imports.Add(null);
                            }
                            else if (strTemp[0].ToUpper().Contains(NameParametr))
                            {
                                search.names = strTemp[1].Trim().ToUpper().Split(new string[] { "||" }, StringSplitOptions.None).ToList();
                            }
                            else
                            {
                                search.attrParams.Add(
                                    new AttrParams()
                                    {
                                        name = strTemp[0].Trim().ToUpper(),
                                        values = strTemp[1].Trim().ToUpper().Split(new string[] { "||" }, StringSplitOptions.None).ToList()
                                    }
                                );
                            }
                        }
                        else
                        {
                            search.unnameds.Add(
                                new AttrParams()
                                {
                                    values = strTemp[0].Trim().ToUpper().Split(new string[] { "||" }, StringSplitOptions.None).ToList()
                                }
                            );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't parse search string. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Info, "Filling search struct is finished");
            return search;
        }
      
        private async Task<PagedResult<Product>> GetProductsPagedResultAsync(Search search, User user, List<int> attributesIds, int? pageSize, int? pageNumber,
            List<int> categoryIds, bool withoutCategory, string sortByPropertyName, bool sortByAsc,
            string ageMonthRange, string ageYearRange, string priceRange, float coefExchange = 1)
        {
            _logger.Log(LogLevel.Info, "Start filling search struct...");

            _logger.Trace("Generating inner request...");
            var sqlInnerRequests = GenerateInnerRequest(search, categoryIds, withoutCategory, ageMonthRange, ageYearRange, priceRange, coefExchange);
            _logger.Trace("Inner request is generated");

            _logger.Trace("Generating count header request...");
            var countHeaderRequest = GenerateHeaderRequest("*", "\"ProductId\"");
            _logger.Trace("Count header request is generated");

            _logger.Trace("Generating products header request...");
            var productsHeaderRequest = GenerateHeaderRequest("*", "rangeSizeProductId");
            _logger.Trace("Products header request is generated");

            var skipTakeRequest = (pageSize != null && pageNumber != null)
                ? $"offset {pageSize * pageNumber} limit {pageSize}"
                : "";

            var watch = System.Diagnostics.Stopwatch.StartNew();
            _logger.Trace("Getting count query...");
            var countQuery = _context.Products.FromSql(countHeaderRequest + $"Where { string.Join(" and ", sqlInnerRequests)}" + $") result Group by result.\"Id\", result.\"NameOrigEng\") superresult) and \"DeleteTime\" is null");
            _logger.Log(LogLevel.Trace, $"Count query successfully received");
            watch.Stop();
            _logger.Debug($"Get count time: {watch.Elapsed.ToString()}");

            _logger.Trace("Getting products query...");
            var query = _context.Products.FromSql(productsHeaderRequest + $"Where { string.Join(" and ", sqlInnerRequests)}" + $") result Group by result.\"Id\", result.\"NameOrigEng\" {skipTakeRequest}) superresult) and \"DeleteTime\" is null");
            _logger.Log(LogLevel.Trace, $"Products query successfully received");

            // Достаем продукты, по найденным Id, подгружая ParentProducts
            watch.Reset();
            watch.Start();
            _logger.Log(LogLevel.Trace, $"Products query and count query by category ids successfully received");

            _logger.Trace("Pagination of products...");
            var products = new PagedResult<Product>
            {
                CurrentPage = pageNumber != null ? (int)pageNumber : 0,
                PageSize = pageSize != null ? (int)pageSize : 0,
                RowCount = int.Parse(countQuery.Count().ToString())
            };
            _logger.Trace("Pagination of products is finished");

            _logger.Trace("Including data to products query...");
            query = query
                .Include(p => p.ParentProduct)
                .ThenInclude(p => p.ProductFiles)
                .Include(p => p.ParentProduct)
                .ThenInclude(p => p.ParentProduct)
                .ThenInclude(p => p.ProductFiles)
                .Include(p => p.ParentProduct.ParentProduct.ProductSearch);
            _logger.Trace("Including data to products is finished");

            if (!string.IsNullOrEmpty(sortByPropertyName) && sortByPropertyName == "БОЦ (базовая оптовая цена предзаказ)")
            {
                query = sortByAsc
                    ? query.OrderBy(p => p.ParentProduct.ParentProduct.ProductSearch.BwpMin)
                    : query.OrderByDescending(p => p.ParentProduct.ParentProduct.ProductSearch.BwpMin);
            }
            else
            {
                query = sortByAsc
                    ? query.OrderBy(p => p.ParentProduct.ParentProduct.ProductSearch.NameOrigEng)
                    : query.OrderByDescending(p => p.ParentProduct.ParentProduct.ProductSearch.NameOrigEng);
            }

            _logger.Trace("Query async loading...");
            await query.LoadAsync();
            _logger.Trace("Query async loading is finished");
            watch.Stop();
            _logger.Debug($"Get products time: {watch.Elapsed.ToString()}");

            // Достаем все Idшники продуктов
            var productsIds = new List<int>();
            var productCategoriesIds = new List<int>();

            _logger.Trace("Getting products models...");
            var models = _context.Products.Local.Select(p => p.ParentProduct?.ParentProduct).Where(p => p != null).Distinct();
            _logger.Trace("Products models successfully received");

            _logger.Trace("Filling properties of products models...");
            foreach (var product in models)
            {
                try
                {
                    productsIds.AddRange(GetPropertiesFromProductTree(product, p => p.Id));
                    productCategoriesIds.AddRange(GetPropertiesFromProductTree(product, p => p.CategoryId.Value).Distinct());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            _logger.Trace("Properties of products models are filled");
            // Выполняем отдельный LoadAsync ибо долго грузит если инклюдить
            watch.Reset();
            watch.Start();
            _logger.Trace("Async loading attribute values...");
            await _context.AttributeValues
                .Where(av => productsIds.Contains(av.ProductId) && (attributesIds == null || attributesIds.Count == 0 || attributesIds.Contains(av.AttributeId)))
                .LoadAsync();
            _logger.Trace("Async loading attribute values is finished");
            _logger.Debug($"Get attributeValues time: {watch.Elapsed.ToString()}");

            // -||-
            watch.Reset();
            watch.Start();

            _logger.Trace("Async loading attributes...");
            await _context.Attributes.Include(a => a.AttributePermissions).Include(a => a.AttributeCategories)
                .Where(a => a.DeleteTime == null && (attributesIds == null || attributesIds.Count == 0 || attributesIds.Contains(a.Id)) && a.AttributeCategories.Any(ac => productCategoriesIds.Contains(ac.CategoryId))).LoadAsync();
            _logger.Trace("Async loading attributes is finished");
            _logger.Debug($"Get attributes time: {watch.Elapsed.ToString()}");

            _logger.Trace("Filtering attribute values...");
            void Executive(Product product)
            {
                product.AttributeValues = product.AttributeValues.Where(
                    av => av.Attribute != null && user.UserRoles.Select(ur => ur.Role).Any(
                        r => av.Attribute.AttributePermissions
                                            .Select(ap => ap.RoleId)
                                            .Contains(r.Id)
                    )
                && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId && ac.ModelLevel == product.ModelLevel)
                )
                .GroupBy(a => new { a.AttributeId, a.ProductId })
                .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                .ToList();
            };
            _logger.Trace("Filtering attribute values is finished");
            watch.Reset();
            watch.Start();

            _logger.Trace($"Filtering attribute values for models...");
            foreach (var model in models)
            {
                DoForEachProductInTree(model, Executive);
            }
            _logger.Trace($"Filtering attribute values for models is finished");

            _logger.Debug($"Filter attributeValues time: {watch.Elapsed.ToString()}");

            _logger.Trace($"Forming result...");
            products.Results = models.Select(p => _transformModelHelper.TransformProduct(p, true)).ToList();
            _logger.Trace($"Forming result is finished");

            _logger.Log(LogLevel.Info, "Filling search struct is finished");
            return products;
        }

        private List<string> GenerateInnerRequest(Search search, List<int> categoryIds, bool withoutCategory, string ageMonthRange, string ageYearRange, string priceRange, float coefExchange)
        {
            _logger.Log(LogLevel.Info, "Start generating inner request...");
            var sqlInnerRequests = new List<string>();

            sqlInnerRequests.Add("pr.\"ParentId\" is null");

            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(priceRange, $"@> (pr.\"BwpMin\"/{coefExchange}) :: int4")));
            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(ageMonthRange, "&& colorR.ageMonthRange")));
            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(ageYearRange, "&& colorR.ageYearRange")));

            _logger.Log(LogLevel.Trace, $"Adding categories({String.Join(", ", categoryIds)}...) and 'withoutCategories' = '{withoutCategory.ToString()}' to search sql script...");
            var categoriesRequestsList = new List<string>();
            if (categoryIds.Any())
                categoriesRequestsList.Add($"colorR.rangesizeCategoryId in ({String.Join(", ", categoryIds.ToList())})");

            if (withoutCategory)
                categoriesRequestsList.Add("colorR.rangesizeCategoryId is null");

            if (categoriesRequestsList.Any())
                sqlInnerRequests.Add(String.Join(" or ", categoriesRequestsList));
            _logger.Log(LogLevel.Trace, $"Categories to search sql script is added");

            if (search.imports.Any())
            {
                _logger.Log(LogLevel.Info, $"Adding imports('{String.Join(", ", search.imports.Where(i => i != null))}') to search sql script...");
                var importsRequestsList = new List<string>();

                if (search.imports.Any(i => i != null))
                    importsRequestsList.Add($"colorR.rangesizeImportId in ({ String.Join(", ", search.imports.Where(i => i != null).ToList())})");

                if (search.imports.Any(i => i == null))
                    importsRequestsList.Add("colorR.rangesizeImportId is null");

                sqlInnerRequests.Add(String.Join(" or ", importsRequestsList));
                _logger.Log(LogLevel.Info, $"Imports('{String.Join(", ", search.imports.Where(i => i != null))}') to search sql script is added");
            }

            foreach (var s in search.sku)
            {
                _logger.Log(LogLevel.Info, $"Adding sku='{s}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSmartSearch) SIMILAR TO '%{s}%' " +
                    $"or text(colorR.colorModelSmartSearch) SIMILAR TO '%{s}%' " +
                    $"or text(pr.\"SmartSearchArray\") SIMILAR TO '%{s}%')");
                _logger.Log(LogLevel.Info, $"Sku='{s}' to search sql script is added");
            }

            foreach (var s in search.names)
            {
                _logger.Log(LogLevel.Info, $"Adding name='{s}' to search sql script...");
                sqlInnerRequests.Add("UPPER(pr.\"Name\") Like '%" + s + "%'");
                _logger.Log(LogLevel.Info, $"Name='{s}' to search sql script is added");
            }

            foreach (var param in search.unnameds)
            {
                _logger.Log(LogLevel.Info, $"Adding unnamed parametr='{param.name}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSmartSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%' " +
                    $"or text(colorR.colorModelSmartSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%' " +
                    $"or text(pr.\"SmartSearchArray\") SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%')");
                _logger.Log(LogLevel.Info, $"Unnamed parametr='{param}' to search sql script is added");
            }

            foreach (var param in search.attrParams)
            {
                _logger.Log(LogLevel.Info, $"Adding attr parametr='{param.name}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%' " +
                    $"or text(colorR.colorModelSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%' " +
                    $"or text(pr.\"FullSearchArray\") SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%')");
                _logger.Log(LogLevel.Info, $"Attr parametr='{param}' to search sql script is added");
            }

            _logger.Log(LogLevel.Info, "Generating inner request is finished");
            return sqlInnerRequests;
        }

        private string GenerateHeaderRequest(string outputParameters, string aggregatedIds)
            => $"select {outputParameters} from public.\"Products\" prod " +
                "where prod.\"Id\" in ( " +
                "select unnest(ids) from( " +
                $"select result.\"Id\", array_agg(result.{aggregatedIds}) as ids, result.\"NameOrigEng\" from( " +
                "select pr.\"Id\", pr.\"BwpMin\", pr.\"NameOrigEng\", pr.\"ProductId\", pr.\"ParentId\", pr.\"FullSearchArray\", pr.\"SmartSearchArray\", colorR.rangesizeCategoryId, colorR.rangesizeImportId, colorR.rangeSizeProductId, colorR.rangesizeSearch, colorR.colorModelSearch, colorR.colorModelSmartSearch, colorR.rangesizeSmartSearch, colorR.ageMonthRange, colorR.ageYearRange " +
                "from public.\"ProductSearch\" pr " +
                "left join( " +
                "select color.\"Id\" as colorId, color.\"ParentId\" as colorParentId, rangesize.\"ImportId\" as rangesizeImportId, rangesize.\"CategoryId\" as rangesizeCategoryId, rangesize.\"ProductId\" as rangeSizeProductId, color.\"FullSearchArray\" as colorModelSearch, rangesize.\"FullSearchArray\" as rangesizeSearch, rangesize.\"AgeMonthRange\" as ageMonthRange, rangesize.\"AgeYearRange\" as ageYearRange, color.\"SmartSearchArray\" as colorModelSmartSearch, rangesize.\"SmartSearchArray\" as rangesizeSmartSearch from public.\"ProductSearch\" color " +
                    "left join( " +
                    "select \"Id\", \"FullSearchArray\", \"SmartSearchArray\", \"ParentId\" as pId, \"ProductId\", \"AgeMonthRange\", \"AgeYearRange\", \"ImportId\", \"CategoryId\" from public.\"ProductSearch\" " +
                    ") rangesize on rangesize.pId = color.\"Id\" " +
                ") colorR on pr.\"Id\" = colorR.colorParentId ";

        private int? ToNullableInt(string s) => int.TryParse(s, out int i) ? (int?)i : null;

        private List<T> GetPropertiesFromProductTree<T>(Product product, Func<Product, T> selector)
            => _treeObjectHelper.GetPropertyFromTreeObject(new List<Product> { product }, selector, p => p.SubProducts);

        /// <summary>
        /// Do some anonymous function in each product in product's tree
        /// </summary>
        /// <param name="product"></param>
        /// <param name="executive"></param>
        private void DoForEachProductInTree(Product product, TreeObjectHelper.TreeExecuteDelegate<Product> executive)
            => _treeObjectHelper.DoForEachInTreeObject(new List<Product> { product }, executive, p => p.SubProducts);

        private string GetStringOrderByRequest(string sortByPropertyName, bool sortByAsc)
        {
            var sortParameterName = "\"NameOrigEng\"";

            if (!string.IsNullOrEmpty(sortByPropertyName) && sortByPropertyName == "БОЦ (базовая оптовая цена предзаказ)")
            {
                sortParameterName = "\"BwpMin\"";
            }

            return $"order by {sortParameterName} " + (sortByAsc ? "asc" : "desc");
        }

        private List<AttributeValue> GetAllAttributeValues(Product product)
        {
            _logger.Log(LogLevel.Info, "Start geting all attribute values...");

            _logger.Trace("Getting level attribute values...");
            var attrValues = GetLevelAttributeValues(product);
            _logger.Trace("Level attribute values successfully received");

            if (product.ParentProduct != null)
            {
                _logger.Trace("Filling attribute values...");
                attrValues.AddRange(GetLevelAttributeValues(product.ParentProduct));

                if (product.ParentProduct.ParentProduct != null)
                    attrValues.AddRange(GetLevelAttributeValues(product.ParentProduct.ParentProduct));
                _logger.Trace("Attribute values are filled");
            }

            _logger.Log(LogLevel.Info, "Geting all attribute values is finished");
            return attrValues;
        }

        private List<AttributeValue> GetLevelAttributeValues(Product product) =>
           product.AttributeValues
            .Where(av => av.Attribute.DeleteTime == null
                         && av.Attribute.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId
                         // && ac.ModelLevel == product.ModelLevel
                         ))
                   .GroupBy(a => a.AttributeId)
                   .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                   .ToList();

        private List<string> GetListStringRangesFromStringOfRanges(string ranges, string conditionString)
        {
            var sqlRangesConditions = new List<string>();

            if (!String.IsNullOrEmpty(ranges))
            {
                foreach (var rangeValue in ranges.Split(","))
                {
                    var from = String.IsNullOrEmpty(rangeValue.Split("-").FirstOrDefault()) ? "NULL" : rangeValue.Split("-").FirstOrDefault();
                    var to = String.IsNullOrEmpty(rangeValue.Split("-").LastOrDefault()) ? "NULL" : rangeValue.Split("-").LastOrDefault();

                    sqlRangesConditions.Add($"(int4range({from}, {to}) {conditionString})");
                }
            }

            return sqlRangesConditions;
        }

        private async Task<float> GetCurrentExchangeCoefficientAsync(string currency)
        {
            var policyExchangeRates = (await _seasonsMsCommunicator.GetDiscountPolicyAsync(_currentSeasonOptions.Id, HttpContext)).ExchangeRates.OrderByDescending(ex => ex.CreateTime).FirstOrDefault();

            var coefExchange = 1.0f;

            if (policyExchangeRates != null)
            {
                if (currency == "USD")
                {
                    if (policyExchangeRates.EurUsd.HasValue)
                        coefExchange = policyExchangeRates.EurUsd.Value;
                    else
                        _logger.Error($"There are no EurUsd coefficient in current season (seasonId: {_currentSeasonOptions.Id})");
                }
                else if (currency == "RUB")
                {
                    if (policyExchangeRates.EurRub.HasValue)
                        coefExchange = policyExchangeRates.EurRub.Value;
                    else
                        _logger.Error($"There are no EurRub coefficient in current season (seasonId: {_currentSeasonOptions.Id})");
                }
            }

            return coefExchange;
        }

    }

}
