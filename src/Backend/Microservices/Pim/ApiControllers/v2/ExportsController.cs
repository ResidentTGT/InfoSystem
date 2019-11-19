using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Company.Common.Models.Pim;
using Company.Pim.Options;
using System.IO;
using Company.Pim.Client.v2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Company.Common.Enums;
using Company.Common.Extensions;
using Company.Pim.Configs;
using Company.Pim.Helpers.v2;

namespace Company.Pim.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class ExportsController : Controller
    {
        private readonly PimContext _context;
        private readonly ExportAttributesIdsOptions _exportAttributesIdsOptions;
        private readonly Logger _logger;


        private IConfiguration _configuration { get; set; }
        private IWebApiCommunicator _httpWebApiCommunicator { get; set; }
        private readonly TransformModelHelpers _transformModelHelper;
        private readonly TreeObjectHelper _treeObjectHelper;
        private readonly SearchHelper _searchHelper;
        private readonly List<ExportTemplate> _exportTemplatesOptions;


        public ExportsController(PimContext context,
            IWebApiCommunicator httpWebApiCommunicator,
            IConfiguration configuration,
            TransformModelHelpers transformModelHelper,
            IOptions<ExportAttributesIdsOptions> exportAttributesIdsOptions,
            TreeObjectHelper treeObjectHelper,
            IOptions<List<ExportTemplate>> exportTemplatesOptions,
            SearchHelper searchHelper)
        {
            _context = context;
            _configuration = configuration;
            _httpWebApiCommunicator = httpWebApiCommunicator;
            _transformModelHelper = transformModelHelper;
            _treeObjectHelper = treeObjectHelper;
            _searchHelper = searchHelper;
            _exportTemplatesOptions = exportTemplatesOptions.Value;
            _exportAttributesIdsOptions = exportAttributesIdsOptions.Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPost("gtin")]
        public async Task<IActionResult> ExportGtin([FromQuery] bool withoutCategory, [FromBody] List<int> productsIds)
        {
            _logger.Log(LogLevel.Debug, $"Start exporting gtin...");

            var categoriesIds = Request.GetIdsFromQuery("categories");
            var productsQuery = productsIds.Any() ? _context.Products.Where(p => p.DeleteTime == null && productsIds.Contains(p.Id)) :
                                                    _searchHelper.SearchProductsQuery(withoutCategory, categoriesIds, Request.Query["searchString"]);

            var exportAttributesIds = new List<int>();
            _exportAttributesIdsOptions.GetType().GetProperties().ToList().ForEach(p =>
            {
                if (p.PropertyType == typeof(int))
                    exportAttributesIds.Add((int)p.GetValue(_exportAttributesIdsOptions));
            }
            );

            _logger.Log(LogLevel.Trace, "Getting products...");

            var products = await productsQuery.Where(p=>p.ModelLevel == ModelLevel.RangeSizeModel).ToListAsync();

            if (!products.Any())
            {
                _logger.Log(LogLevel.Error, $"Products by specified params were not found");
                return NotFound($"Products by specified params were not found");
            }

            _logger.Log(LogLevel.Trace, "Products were successfully received");
            var productsHeads = products.Select(product => _treeObjectHelper.GetHeadOfTreeObject(product, p => p.ParentProduct))
                                        .GroupBy(p => p.Id)
                                        .Select(g => g.First())
                                        .ToList();

            var allProducts = _treeObjectHelper.GetPropertyFromTreeObject(productsHeads, p => p, p => p.SubProducts);
            var allProductsIds = allProducts.Select(p => p.Id);

            _logger.Log(LogLevel.Trace, $"Loading attributeValues by product tree ids('{String.Join(',', productsIds)}')...");
            await _context.AttributeValues
                .Include(av => av.Attribute)
                .Include(av => av.ListValue)
                .Where(av => exportAttributesIds.Contains(av.AttributeId) && allProductsIds.Contains(av.ProductId) && av.Attribute.DeleteTime == null)
                .OrderByDescending(av => av.CreateTime)
                .LoadAsync();

            _logger.Log(LogLevel.Trace, "AttributeValues were successfully loaded");

            _logger.Log(LogLevel.Trace, "Grouping attributes values by attribute id...");
            foreach (var product in allProducts)
            {
                product.AttributeValues = product.AttributeValues
                    .GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();
            }
            _logger.Log(LogLevel.Trace, "Attributes values were successfully grouped");

            _logger.Log(LogLevel.Trace, "Export to excel...");
            byte[] fileContents;
            var fileName = $"Excel-{DateTime.Now:yyyy-MM-dd}.xlsx";

            using (var pck = new ExcelPackage())
            {
                var ws = pck.Workbook.Worksheets.Add("Данные");
                AddHeadersRows(ws);
                await FillProductData(ws, products, exportAttributesIds);
                _logger.Log(LogLevel.Trace, "Converting excel package to byte array");
                fileContents = pck.GetACmpyteArray();
            }

            _logger.Log(LogLevel.Info, $"Export {fileName} was successfully finished.");

            return File(
                fileContents: fileContents,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: fileName
            );
        }

        [HttpPost]
        public async Task<IActionResult> CreateExportFile([FromQuery] int? templateCategoryId, [FromQuery] bool withoutCategory, [FromBody] List<int> productsIds)
        {
            _logger.Log(LogLevel.Debug, $"Start export of products.");

            _logger.Log(LogLevel.Trace, $"Getting products from DB...");

            var categories = Request.GetIdsFromQuery("categories");

            var productsQuery = productsIds.Any() ? _context.Products.Where(p => p.DeleteTime == null && productsIds.Contains(p.Id)) :
                _searchHelper.SearchProductsQuery(withoutCategory, categories, Request.Query["searchString"]);


            var products = await productsQuery
                .Include(p => p.ParentProduct.ParentProduct)
                .Include(p => p.SubProducts)
                .ThenInclude(sp => sp.SubProducts)
                .ToListAsync();

            if (!products.Any())
            {
                _logger.Log(LogLevel.Error, $"Products by specified params were not found");
                return NotFound($"Products by specified params were not found");
            }
            _logger.Log(LogLevel.Trace, "Products were successfully received");

            var treeObjectHelper = new TreeObjectHelper();

            var parentProducts = treeObjectHelper.GetPropertyFromTreeObject(products, p => p,
                p => p.ParentProduct != null ? new List<Product> { p.ParentProduct } : new List<Product>()
            ).Where(p => p.ParentId == null).GroupBy(p => p.Id).Select(p => p.First()).ToList();

            products = treeObjectHelper.GetPropertyFromTreeObject(parentProducts, p => p, p => p.SubProducts);

            _logger.Log(LogLevel.Trace, $"Getting categories, attributeValues and listValues from DB...");
            var allProductsIds = products.Select(p => p.Id);
            await _context.AttributeValues.Include(av => av.Attribute).Where(av => allProductsIds.Contains(av.ProductId)).LoadAsync();
            await _context.Categories.LoadAsync();
            _logger.Log(LogLevel.Trace, $"Getting categories, attributeValues and listValues from DB is finished");

            var listValuesIds = products.SelectMany(p => p.AttributeValues.Where(av => av.ListValueId != null).Select(av => av.ListValueId)).Distinct();
            await _context.ListValues.Where(lv => listValuesIds.Contains(lv.Id)).LoadAsync();

            _logger.Log(LogLevel.Trace, $"Getting attribute values...");
            foreach (var product in products)
            {
                product.AttributeValues = product.AttributeValues.Where(av => av.Attribute.DeleteTime == null)
                    .GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                    .ToList();


            }
            _logger.Log(LogLevel.Trace, "Attribute values successfully received");

            _logger.Log(LogLevel.Trace, $"Categories, attributeValues, listValues and products have got from DB.");

            try
            {
                _logger.Log(LogLevel.Trace, $"Creation of Excel file started...");

                using (var package = new ExcelPackage(new FileInfo("./exportFile")))
                {
                    var productGroups = products.GroupBy(p => p.ModelLevel).OrderBy(p => p.Key);
                    foreach (var productGroup in productGroups)
                    {
                        var workSheet = package.Workbook.Worksheets.Add(GetListName(productGroup.Key));

                        var row = 2;
                        foreach (var letter in ExportExcelConfig.Headers)
                        {
                            workSheet.Cells[$"{letter.Key}1"].Value = letter.Value;
                        }

                        workSheet.Cells[1, 1, 1, ExportExcelConfig.Headers.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;


                        var bindCells = new Dictionary<int, int>();
                        var countColumnNumMax = ExportExcelConfig.Headers.Count + 1;

                        var count = 1;
                        foreach (var pr in productGroup)
                        {
                            _logger.Log(LogLevel.Trace, $"Getting info from product {count} (Id:{pr.Id})...");
                            var prodCategories = TransformCategoryWithParent(pr.Category).ToArray();

                            workSheet.Cells[$"A{row}"].Value = pr.Sku;
                            workSheet.Cells[$"C{row}"].Value = pr.ParentProduct?.Sku;
                            workSheet.Cells[$"D{row}"].Value = pr.Name;

                            if (productGroup.Key != ModelLevel.RangeSizeModel)
                                workSheet.Cells[$"B{row}"].Value = pr.Sku;

                            if (pr.Category != null)
                            {
                                workSheet.Cells[$"E{row}"].Value = string.Join("$", prodCategories.Select(c => c.Name));

                                var iIndex = Enumerable.Range(ExportExcelConfig.Headers.Keys.LastOrDefault() - 4, 5).ToArray()[0];
                                foreach (var letter in Enumerable.Range(ExportExcelConfig.Headers.Keys.LastOrDefault() - 4, 5).ToArray())
                                    workSheet.Cells[$"{(char)letter}{row}"].Value = prodCategories.Length > letter - iIndex ? prodCategories[letter - iIndex].Name : null;
                            }
                            var sortedAttributeValues = await SortByTemplate(pr.AttributeValues, pr.ModelLevel, templateCategoryId);
                            foreach (var av in sortedAttributeValues)
                            {
                                if (bindCells.ContainsKey(av.Attribute.Id))
                                {
                                    workSheet.Cells[row, bindCells[av.Attribute.Id]].Value = (av.StrValue + " " + av.NumValue + " " + av.DateValue + " " + (av.BoolValue != null ? ((bool)av.BoolValue ? "Да" : "Нет") : null) + " " + av.ListValue?.Value).Trim();
                                }
                                else
                                {
                                    bindCells.Add(av.Attribute.Id, countColumnNumMax);
                                    workSheet.Cells[1, countColumnNumMax].Value = av.Attribute.Name;
                                    workSheet.Cells[row, countColumnNumMax].Value = (av.StrValue + " " + av.NumValue + " " + av.DateValue + " " + (av.BoolValue != null ? ((bool)av.BoolValue ? "Да" : "Нет") : null) + " " + av.ListValue?.Value).Trim();
                                    countColumnNumMax++;
                                }
                            }
                            _logger.Log(LogLevel.Trace, $"Info from product (Id:{pr.Id}) received and has been written.");
                            row++;
                            count++;
                        }
                        workSheet.Cells.AutoFitColumns();
                        for (int col = ExportExcelConfig.Headers.Count; col <= workSheet.Dimension.End.Column; col++)
                        {
                            if (workSheet.Column(col).Width > 100)
                                workSheet.Column(col).Width = 100;
                        }
                    }

                    package.Stream.Seek(0, SeekOrigin.Begin);

                    _logger.Log(LogLevel.Trace, $"Creation of Excel file finished.");

                    _logger.Log(LogLevel.Debug, $"Export of products {String.Join(',', productsIds)} method is finished.");
                    return File(
                        fileContents: package.GetACmpyteArray(),
                        contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileDownloadName: "export.xlsx"
                    );
                }


            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Ошибка генерации файла экспорта: {e.Message}");
                return BadRequest($"Ошибка генерации файла экспорта: {e.Message}");
            }
        }

        public List<Category> TransformCategoryWithParent(Category category)
        {
            var categories = category != null && category.ParentCategory != null ? TransformCategoryWithParent(category.ParentCategory) : new List<Category>();
            categories.Add(category);
            return categories;
        }

        private string GetListName(ModelLevel modelLevel)
        {
            switch (modelLevel)
            {
                case ModelLevel.Model: return "Модели";
                case ModelLevel.ColorModel: return "Цвето-модели";
                case ModelLevel.RangeSizeModel: return "Размерные ряды";
                default: return null;
            }
        }

        private async Task<ICollection<AttributeValue>> SortByTemplate(ICollection<AttributeValue> attributeValues, ModelLevel modelLevel, int? categoryId)
        {
            if (categoryId == null)
                return attributeValues;

            var template = _exportTemplatesOptions.FirstOrDefault(et => et.CategoryId == categoryId);
            if (template == null)
            {
                _logger.Error($"There is no template for category with Id={categoryId}");
                throw new Exception($"There is no template for category with Id={categoryId}");
            }

            var attrIds = new List<int>();
            var sortedAttributeValues = new List<AttributeValue>();
            switch (modelLevel)
            {
                case ModelLevel.Model: attrIds.AddRange(template.Model); break;
                case ModelLevel.ColorModel: attrIds.AddRange(template.ColorModel); break;
                case ModelLevel.RangeSizeModel: attrIds.AddRange(template.RangeSizeModel); break;
            }
            var attrValuesDict = attributeValues.ToDictionary(av => av.AttributeId);
            var attrDict = await _context.Attributes.Where(a => attrIds.Contains(a.Id)).ToDictionaryAsync(a => a.Id);

            attrIds.ForEach(attrId =>
            {
                if (attrValuesDict.ContainsKey(attrId))
                    sortedAttributeValues.Add(attrValuesDict[attrId]);
                else sortedAttributeValues.Add(new AttributeValue()
                {
                    Attribute = attrDict[attrId]
                });
            });
            return sortedAttributeValues;
        }

        private async Task FillProductData(ExcelWorksheet ws, List<Product> products, List<int> exportAttributesIds)
        {

            _logger.Log(LogLevel.Debug, "Start filling product data...");
            var row = ExportGs1Config.HeaderHeight + 1;


            foreach (var product in products)
            {
                _logger.Log(LogLevel.Trace, "Getting attributeCategories by ids...");
                var attrCategories = await _context.AttributeCategories.Where(ac => ac.CategoryId == product.CategoryId && exportAttributesIds.Contains(ac.AttributeId))
                    .ToDictionaryAsync(ac => ac.AttributeId);
                _logger.Log(LogLevel.Trace, "AttributeCategories were successfully got");

                var vendorCode = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.VendorCode, attrCategories)?.StrValue;
                var brand = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.Brand, attrCategories)?.ListValue?.Value;
                var sku = product.Sku;
                var color = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.Color, attrCategories)?.ListValue?.Value;
                var size = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.Size, attrCategories)?.StrValue;
                var date = GetPublicationDate(product, attrCategories);

                ws.Cells[row, 4].Value = sku;

                ws.Cells[row, 5].Style.Numberformat.Format = "dd.MM.yyyy";
                ws.Cells[row, 5].Value = date;

                ws.Cells[row, 6].Value = $"{vendorCode} {product.Name} {brand} {color} {size}";
                ws.Cells[row, 7].Value = brand;
                ws.Cells[row, 8].Value = _exportAttributesIdsOptions.Tin;
                ws.Cells[row, 10].Value = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.ShoeType, attrCategories)?.ListValue?.Value;
                ws.Cells[row, 11].Value = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.ShoesUpMaterial, attrCategories)?.StrValue;
                ws.Cells[row, 12].Value = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.ShoesLiningMaterial, attrCategories)?.StrValue;
                ws.Cells[row, 13].Value = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.ShoesDownMaterial, attrCategories)?.StrValue;
                ws.Cells[row, 14].Value = color;
                ws.Cells[row, 15].Value = GetSize(size);

                row++;
            }
            _logger.Log(LogLevel.Trace, "Products were successfully filled");
            _logger.Log(LogLevel.Debug, "Fill product data method is finished");
        }

        private AttributeValue GetAttributeFromProductTree(Product product, int attrId, Dictionary<int, AttributeCategory> attributeCategories)
        {
            if (!attributeCategories.ContainsKey(attrId))
            {
                _logger.Error($"AttributeCategory not found for attributeId {attrId} and categoryId {product.CategoryId}");
                throw new Exception($"AttributeCategory not found for attributeId {attrId} and categoryId {product.CategoryId}");
            }

            var attributeModelLevel = attributeCategories[attrId].ModelLevel;
            var head = _treeObjectHelper.GetHeadOfTreeObject(product, p => p.ParentProduct);
            ICollection<AttributeValue> attrValues;
            switch (attributeModelLevel)
            {
                case ModelLevel.Model: attrValues = head.AttributeValues; break;
                case ModelLevel.ColorModel: attrValues = head.SubProducts.FirstOrDefault()?.AttributeValues; break;
                case ModelLevel.RangeSizeModel: attrValues = head.SubProducts.FirstOrDefault()?.SubProducts.FirstOrDefault()?.AttributeValues; break;
                default: return null;
            }

            return attrValues?.FirstOrDefault(av => av.AttributeId == attrId);
        }

        private string GetSize(string sizeStr)
        {
            if (sizeStr == null)
                return "";

            var success = double.TryParse(sizeStr, NumberStyles.Any, null, out var size);
            if (success)
            {
                if (size > 19.5 && size < 49.5)
                {
                    var sizeText = size > Math.Floor(size) ? size.ToString("F1") : ((int)size).ToString();
                    return $"<{390000000 + (size - 19) / 0.5}> {sizeText}";
                }
                return "<390000099> НЕТ В СПРАВОЧНИКЕ";
            }
            else
            {
                return "";
            }
        }

        private string GetPublicationDate(Product product, Dictionary<int, AttributeCategory> attributeCategories)
        {
            var publicationDate = GetAttributeFromProductTree(product, _exportAttributesIdsOptions.PublicationDate, attributeCategories)?.DateValue;
            if (publicationDate == null)
            {
                _logger.Log(LogLevel.Trace, "PublicationDate attribute is null");
                var attribute = product.AttributeValues.FirstOrDefault(av => av.AttributeId == _exportAttributesIdsOptions.Season);
                if (attribute == null)
                {
                    _logger.Log(LogLevel.Warn, "Season and publication date attiributes are null");
                    return "";
                }

                publicationDate = DateTime.Now;
                if (attribute.ListValue.Value.Trim().ToUpper().StartsWith("FW"))
                    publicationDate = new DateTime(publicationDate.Value.Year + 1, 3, 1);
                else if (attribute.ListValue.Value.Trim().ToUpper().StartsWith("SS"))
                    publicationDate = new DateTime(publicationDate.Value.Year, 9, 1);
            }
            _logger.Log(LogLevel.Trace, $"Publication date successfully setted to {publicationDate.Value:dd.MM.yyyy}");
            return publicationDate.Value.ToString("dd.MM.yyyy");
        }

        private void AddHeadersRows(ExcelWorksheet ws)
        {
            _logger.Log(LogLevel.Trace, "Filling headers...");
            var column = 1;
            foreach (var header in ExportGs1Config.Headers)
            {
                ws.Cells[1, column, 1, column].Value = header.Code;
                ws.Row(1).Height = 0;

                ws.Cells[2, column, 2, column].Value = column;
                ws.Cells[3, column, 3, column].Value = header.Name;
                ws.Row(3).Height = 28.5;

                ws.Cells[4, column, 4, column].Value = header.Description;
                ws.Row(4).Height = 114.75;

                ws.Cells[5, column, 5, column].Value = header.Required;
                ws.Cells[6, column, 6, column].Value = header.Type;
                ws.Row(6).Height = 43.5;

                ws.Column(column).AutoFit(10, 60);
                column++;
            }

            ws.Cells[3, column - 1, 5, column - 1].Merge = true;

            ws.View.FreezePanes(ExportGs1Config.HeaderHeight + 1, 3);

            ws.Cells[1, 1, ExportGs1Config.HeaderHeight, ExportGs1Config.Headers.Count].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            ws.Cells[1, 1, ExportGs1Config.HeaderHeight, ExportGs1Config.Headers.Count].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ws.Cells[1, 1, ExportGs1Config.HeaderHeight, ExportGs1Config.Headers.Count].Style.WrapText = true;

            ws.Cells[3, 1, 4, ExportGs1Config.Headers.Count].Style.Font.Bold = true;
            ws.Cells[5, 1, ExportGs1Config.HeaderHeight, ExportGs1Config.Headers.Count].Style.Font.Italic = true;
            _logger.Log(LogLevel.Trace, "Headers were successfully filled");
        }
    }
}