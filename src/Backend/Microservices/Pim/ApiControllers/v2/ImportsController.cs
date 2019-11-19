using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog;
using OfficeOpenXml;
using Company.Common;
using Company.Common.Enums;
using Company.Common.Extensions;
using Company.Common.Models.Pim;
using Company.Common.Options;
using Company.Pim.Client.v2;
using Company.Pim.Configs;
using Company.Pim.Helpers.v2;
using Company.Pim.Options;
using Company.Pim.Parsing;
using File = Company.Common.Models.FileStorage.File;

namespace Company.Pim.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/[controller]")]
    public class ImportsController : Controller
    {
        private readonly PimContext _context;
        private readonly SkuGenerator _skuGenerator;

        private readonly AttributesIdsOptions _attributesIdsOptions;
        private readonly ExportAttributesIdsOptions _exportAttributesIdsOptions;
        private readonly Logger _logger;
        private IFileStorageMsCommunicator _httpFileStorageMsCommunicator { get; set; }

        public ImportsController(PimContext context,
            IFileStorageMsCommunicator httpFileStorageMsCommunicator,
            SkuGenerator skuGenerator,
            IOptions<ExportAttributesIdsOptions> exportAttributesIdsOptions,
            IOptions<AttributesIdsOptions> attributesIdsOptions
            )
        {
            _context = context;
            _skuGenerator = skuGenerator;
            _attributesIdsOptions = attributesIdsOptions.Value;
            _exportAttributesIdsOptions = exportAttributesIdsOptions.Value;
            _httpFileStorageMsCommunicator = httpFileStorageMsCommunicator;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _context.Imports.Where(c => c.DeleteTime == null)
                .OrderByDescending(i => i.CreateTime)
                .ToListAsync());
        }

        [HttpGet("{id}/error")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok((await _context.Imports.FirstOrDefaultAsync(c => c.Id == id)).Error);
        }

        [HttpPost]
        public async Task<IActionResult> Create()
        {
            _logger.Log(LogLevel.Debug, "Start creating import...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Getting ids of necessary attributes...");
            var necessaryAttributesStr = Request.Query["necessaryAttributes"].ToString();
            var necessaryAttributes = necessaryAttributesStr != "" ? necessaryAttributesStr.Split(',').Select(int.Parse).ToList() : new List<int>();
            _logger.Log(LogLevel.Trace, $"Ids('{necessaryAttributes}') of necessary attributes successfully received");

            _logger.Log(LogLevel.Trace, "Getting file from request...");
            var reqFile = HttpContext.Request.Form.Files.FirstOrDefault();
            _logger.Log(LogLevel.Trace, $"File {reqFile.Name} from request has been got.");

            var extractPathFile = Path.GetTempPath();
            var importName = DateTime.Now.Ticks.ToString();

            try
            {
                _logger.Log(LogLevel.Trace, $"Saving archive file {reqFile.Name} by name {importName}.zip on server...");
                using (var file = System.IO.File.Create(Path.Combine(extractPathFile, $"{importName}.zip")))
                {
                    try
                    {
                        reqFile.CopyTo(file);
                        _logger.Log(LogLevel.Trace, $"Archive file {importName}.zip saved on server.");
                    }
                    catch (Exception e)
                    {
                        _logger.Log(LogLevel.Fatal, $"Could not save file {reqFile.Name} by name {importName}.zip  on server.\n{e.Message}");
                        throw new Exception("Не удалось сохранить файл архива на диск.");
                    }
                }
                using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(extractPathFile, $"{importName}.zip")))
                {
                    _logger.Log(LogLevel.Trace, $"Opening archive file {importName}.zip. ...");
                    if (!archive.Entries.Any(e => e.FullName == "Products.xlsx"))
                    {
                        _logger.Log(LogLevel.Fatal, $"Archive file {importName}.zip is not include file Products.xlsx.");
                        return BadRequest("Архивированный файл не содержит файла Products.xlsx.");
                    }

                    var importDirectory = Path.Combine(extractPathFile, importName);
                    _logger.Log(LogLevel.Trace, $"Creating directory {importDirectory} on server.");
                    if (!Directory.Exists(importDirectory))
                        Directory.CreateDirectory(importDirectory);
                    _logger.Log(LogLevel.Trace, $"Directory {importDirectory} created on server.");

                    var imagesDirectory = Path.Combine(importDirectory, "Images");
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == "Products.xlsx")
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(importDirectory, entry.FullName));
                            _logger.Log(LogLevel.Trace, $"Extracting Products.xlsx to {destinationPath}.");
                            if (destinationPath.StartsWith(importDirectory, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath, true);
                            _logger.Log(LogLevel.Trace, $"Products.xlsx extracted to {destinationPath}.");
                        }

                        if (!Directory.Exists(imagesDirectory))
                        {
                            _logger.Log(LogLevel.Trace, $"Creating directory {imagesDirectory} on server.");
                            Directory.CreateDirectory(imagesDirectory);
                            _logger.Log(LogLevel.Trace, $"Directory {imagesDirectory} created on server.");
                        }
                        if (entry.FullName.StartsWith("Images/") && entry.FullName != "Images/")
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(imagesDirectory, entry.Name));

                            if (destinationPath.StartsWith(imagesDirectory, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath, true);
                        }
                    }
                    _logger.Log(LogLevel.Trace, $"All images extracted to {imagesDirectory}.");
                }

                File importFile;

                _logger.Log(LogLevel.Trace, $"Saving {reqFile.Name} to FileStorage.");
                try
                {
                    importFile = _httpFileStorageMsCommunicator.SaveFile(HttpContext);
                    _logger.Log(LogLevel.Trace, $"{reqFile.Name} saved to FileStorage.");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Fatal, $"Could not save {reqFile.Name} to FileStorage.\n{e.Message}");
                    throw new Exception("Не удалось сохранить файл импорта в файловое хранилище.");
                }

                _logger.Log(LogLevel.Trace, $"Creating import entity...");
                var import = new Import()
                {
                    CreateTime = DateTime.Now,
                    CreatorId = userId,
                    FileId = importFile.Id,
                    Name = importFile.Name,
                };
                _context.Imports.Add(import);
                await _context.SaveChangesAsync();
                _logger.Log(LogLevel.Info, $"Import entity {import.Name} created.");
                try
                {
                    _logger.Log(LogLevel.Trace, $"Starting parsing {import.Name}...");
                    var imagesPath = Path.Combine(extractPathFile, importName, "Images");
                    using (var fileStream = new FileStream(Path.Combine(extractPathFile, importName, "Products.xlsx"), FileMode.Open))
                        new ImportParser(_context, import, _skuGenerator, _httpFileStorageMsCommunicator, imagesPath, necessaryAttributes.ToList()).ParseFile(fileStream);

                    _logger.Log(LogLevel.Info, $"Parsing {import.Name} finished. Total products: {import.TotalCount}. Success: {import.SuccessCount}");
                    import.FinishedSuccess = import.TotalCount == import.SuccessCount;
                }
                catch (Exception e)
                {
                    import.Error = e.Message;
                    import.FinishedSuccess = false;
                    import.ModelCount = import.RangeSizeModelCount = import.ColorModelCount = import.ModelSuccessCount = import.ColorModelSuccessCount = import.RangeSizeModelSuccessCount = 0;
                    _logger.Log(LogLevel.Error, $"Parsing {import.Name} failed. {e.Message}");
                    throw new Exception(e.Message);
                }
                finally
                {
                    if (import.FinishedSuccess == true)
                    {
                        _logger.Log(LogLevel.Trace, $"Loading attributes by ids...");
                        var attrIds = _context.Products.Local.Select(p => p.AttributeValues.Select(av => av.AttributeId)).SelectMany(a => a).Distinct();
                        await _context.Attributes.Where(a => attrIds.Contains(a.Id)).LoadAsync();
                        _logger.Log(LogLevel.Trace, $"Attributes successfully loaded");

                        _logger.Log(LogLevel.Trace, $"Updating products SearchString and calculating BWP pure and RRC pure...");
                        _context.Products.Local.ForEach(p =>
                        {
                            CalculateBwpRrcPure(p, userId);
                            p.UpdateSearchStringArray();
                        });
                        _logger.Log(LogLevel.Trace, $"Products SearchString was successfully updated");
                    }
                    else
                    {
                        var changedEntities = _context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged && e.Metadata.ClrType != typeof(Import)).ToList();
                        foreach (var entityEntry in changedEntities)
                        {
                            entityEntry.State = EntityState.Detached;
                        }
                    }

                    _logger.Log(LogLevel.Trace, $"Saving products and import {import.Name} in DB...");
                    await _context.SaveChangesAsync();
                    _logger.Log(LogLevel.Info, $"Products and import {import.Name} saved in DB.");
                }

                _logger.Log(LogLevel.Info, $"Import {importFile.Name} successfully finished.");
                _logger.Log(LogLevel.Debug, "Creating import is finished");
                return Ok();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Fatal, $"Import failed. Exception: {e.Message}");
                return BadRequest(e.Message);
            }
            finally
            {
                var importPath = Path.Combine(Path.GetTempPath(), importName);
                var importZipPath = Path.Combine(Path.GetTempPath(), $"{importName}.zip");
                _logger.Log(LogLevel.Trace, $"Deleting {importPath} and {importZipPath}...");
                if (Directory.Exists(importPath))
                    Directory.Delete(importPath, true);
                if (System.IO.File.Exists(importZipPath))
                    System.IO.File.Delete(importZipPath);
                _logger.Log(LogLevel.Trace, $"{importPath} and {importZipPath} deleted.");
            }
        }

        [HttpPost("old")]
        public async Task<IActionResult> CreateOldImport()
        {
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Info, "Import started.");

            _logger.Log(LogLevel.Trace, "Getting file from request...");
            var reqFile = HttpContext.Request.Form.Files.FirstOrDefault();
            _logger.Log(LogLevel.Trace, $"File {reqFile.Name} from request has been got.");

            var extractPathFile = Path.GetTempPath();
            var importName = DateTime.Now.Ticks.ToString();

            try
            {
                _logger.Log(LogLevel.Trace, $"Saving archive file {reqFile.Name} by name {importName}.zip on server...");
                using (var file = System.IO.File.Create(Path.Combine(extractPathFile, $"{importName}.zip")))
                {
                    try
                    {
                        reqFile.CopyTo(file);
                        _logger.Log(LogLevel.Trace, $"Archive file {importName}.zip saved on server.");
                    }
                    catch (Exception e)
                    {
                        _logger.Log(LogLevel.Fatal, $"Could not save file {reqFile.Name} by name {importName}.zip  on server.\n{e.Message}");
                        throw new Exception("Не удалось сохранить файл архива на диск.");
                    }
                }
                using (ZipArchive archive = ZipFile.OpenRead(Path.Combine(extractPathFile, $"{importName}.zip")))
                {
                    _logger.Log(LogLevel.Trace, $"Opening archive file {importName}.zip. ...");
                    if (!archive.Entries.Any(e => e.FullName == "Products.xlsx"))
                    {
                        _logger.Log(LogLevel.Fatal, $"Archive file {importName}.zip is not include file Products.xlsx.");
                        return BadRequest("Архивированный файл не содержит файла Products.xlsx.");
                    }

                    var importDirectory = Path.Combine(extractPathFile, importName);
                    _logger.Log(LogLevel.Trace, $"Creating directory {importDirectory} on server.");
                    if (!Directory.Exists(importDirectory))
                        Directory.CreateDirectory(importDirectory);
                    _logger.Log(LogLevel.Trace, $"Directory {importDirectory} created on server.");

                    var imagesDirectory = Path.Combine(importDirectory, "Images");
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName == "Products.xlsx")
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(importDirectory, entry.FullName));
                            _logger.Log(LogLevel.Trace, $"Extracting Products.xlsx to {destinationPath}.");
                            if (destinationPath.StartsWith(importDirectory, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath, true);
                            _logger.Log(LogLevel.Trace, $"Products.xlsx extracted to {destinationPath}.");
                        }

                        if (!Directory.Exists(imagesDirectory))
                        {
                            _logger.Log(LogLevel.Trace, $"Creating directory {imagesDirectory} on server.");
                            Directory.CreateDirectory(imagesDirectory);
                            _logger.Log(LogLevel.Trace, $"Directory {imagesDirectory} created on server.");
                        }
                        if (entry.FullName.StartsWith("Images/") && entry.FullName != "Images/")
                        {
                            string destinationPath = Path.GetFullPath(Path.Combine(imagesDirectory, entry.Name));

                            if (destinationPath.StartsWith(imagesDirectory, StringComparison.Ordinal))
                                entry.ExtractToFile(destinationPath, true);
                        }
                    }
                    _logger.Log(LogLevel.Trace, $"All images extracted to {imagesDirectory}.");
                }

                File importFile;

                _logger.Log(LogLevel.Trace, $"Saving {reqFile.Name} to FileStorage.");
                try
                {
                    importFile = _httpFileStorageMsCommunicator.SaveFile(HttpContext);
                    _logger.Log(LogLevel.Trace, $"{reqFile.Name} saved to FileStorage.");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Fatal, $"Could not save {reqFile.Name} to FileStorage.\n{e.Message}");
                    throw new Exception("Не удалось сохранить файл импорта в файловое хранилище.");
                }

                _logger.Log(LogLevel.Trace, $"Creating import entity...");
                var import = new Import()
                {
                    CreateTime = DateTime.Now,
                    CreatorId = userId,
                    FileId = importFile.Id,
                    Name = importFile.Name,
                    RangeSizeModelCount = 0,
                    RangeSizeModelSuccessCount = 0,
                    ColorModelCount = 0,
                    ColorModelSuccessCount = 0,
                    ModelCount = 0,
                    ModelSuccessCount = 0,
                    ErrorCount = 0
                };
                _context.Imports.Add(import);
                await _context.SaveChangesAsync();
                _logger.Log(LogLevel.Info, $"Import entity {import.Name} created.");
                try
                {
                    _logger.Log(LogLevel.Trace, $"Starting parsing {import.Name}...");
                    using (var fileStream = new FileStream(Path.Combine(extractPathFile, importName, "Products.xlsx"), FileMode.Open))
                        new OldImportParser(_context, import, _skuGenerator, _httpFileStorageMsCommunicator, Path.Combine(extractPathFile, importName, "Images")).ParseFile(fileStream);

                    _logger.Log(LogLevel.Info, $"Parsing {import.Name} finished. Total products: {import.TotalCount}. Success: {import.SuccessCount}");
                    import.FinishedSuccess = import.TotalCount == import.SuccessCount;
                }
                catch (Exception e)
                {
                    import.Error = e.Message;
                    import.FinishedSuccess = false;
                    _logger.Log(LogLevel.Error, $"Parsing {import.Name} failed. {e.Message}");
                    throw new Exception(e.Message);
                }
                finally
                {
                    _logger.Log(LogLevel.Trace, $"Saving products and import {import.Name} in DB...");
                    await _context.SaveChangesAsync();
                    _logger.Log(LogLevel.Info, $"Products and import {import.Name} saved in DB.");
                }

                _logger.Log(LogLevel.Info, $"Import {importFile.Name} successfully finished.");

                return Ok();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Fatal, $"Import failed. Exception: {e.Message}");
                return BadRequest(e.Message);
            }
            finally
            {
                var importPath = Path.Combine(Path.GetTempPath(), importName);
                var importZipPath = Path.Combine(Path.GetTempPath(), $"{importName}.zip");
                _logger.Log(LogLevel.Trace, $"Deleting {importPath} and {importZipPath}...");
                if (Directory.Exists(importPath))
                    Directory.Delete(importPath, true);
                if (System.IO.File.Exists(importZipPath))
                    System.IO.File.Delete(importZipPath);
                _logger.Log(LogLevel.Trace, $"{importPath} and {importZipPath} deleted.");
            }
        }


        [HttpPost("gtin")]
        public async Task<IActionResult> ImportGtin()
        {
            _logger.Log(LogLevel.Debug, "Start importing gtin...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            var file = Request.Form.Files.FirstOrDefault();
            if (file == null)
            {
                _logger.Log(LogLevel.Error, $"File not found.");
                return NotFound("File not found");
            }

            _logger.Log(LogLevel.Info, "Import started.");

            _logger.Log(LogLevel.Trace, "Reading file...");
            try
            {
                using (var pck = new ExcelPackage(file.OpenReadStream()))
                {
                    _logger.Log(LogLevel.Trace, "Checking worksheet...");
                    var ws = pck.Workbook.Worksheets.FirstOrDefault();

                    if (ws == null)
                    {
                        _logger.Log(LogLevel.Error, $"Worksheets were not found.");
                        return NotFound("Worksheets were not found");
                    }
                    _logger.Log(LogLevel.Trace, "Checking worksheet is finished");

                    await UpdateProducts(ws, userId);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Import failed. Exception: {e.Message}");
                return BadRequest(e.Message);
            }

            _logger.Log(LogLevel.Info, "Import was successfully completed!");
            _logger.Log(LogLevel.Debug, "Importing gtin is finished");
            return Ok();
        }

        private void CalculateBwpRrcPure(Product product, int userId)
        {
            _logger.Info($"Calculating BWP pure and RRC for product with id={product.Id}...");
            _logger.Trace("Getting LCP, LC, BWP, RRC...");
            var lcp = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Lcp)?.NumValue;
            var bwp = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Bwp)?.NumValue;
            var rrc = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Rrc)?.NumValue;

            var vatListValue = product.AttributeValues.FirstOrDefault(p => p.AttributeId == _attributesIdsOptions.Vat)?.ListValue?.Value;
            var vat = vatListValue != null ? double.Parse(vatListValue) : (double?)null;

            var mustBeWithValueForCalculation = new[] { lcp, bwp, rrc, vat };
            if (mustBeWithValueForCalculation.Contains(null))
            {
                _logger.Log(LogLevel.Warn, $"Couldn't calculate BWP pure and RRC pure. Becase some properties is null:BWP={bwp}, RRC={rrc}, LCP={lcp}, VAT={vat}");
                return;
            }
            _logger.Trace("LCP, BWP, RRC, VAT sucessfully received");

            _logger.Trace($"Calculation BWP pure, RRC pure, LC and adding to product with id={product.Id}...");

            var lc = lcp * (1 + vat / 100);


            var bwpPure = bwp - lc + lcp;
            var rrcPure = rrc - lc + lcp;

            RecalculateAttributeValue(_attributesIdsOptions.BwpPure, bwpPure, product, userId);
            RecalculateAttributeValue(_attributesIdsOptions.RrcPure, rrcPure, product, userId);
            RecalculateAttributeValue(_attributesIdsOptions.Lc, lc, product, userId);

            _logger.Info($"LC,BWP pure and RRC pure is calculated for product with id={product.Id}");
        }
        private void RecalculateAttributeValue(int? attributeId, double? attributeValue, Product product, int creatorId)
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
                    ProductId = product.Id,
                    CreateTime = DateTime.Now,
                    CreatorId = creatorId
                });
            }
        }
        private async Task UpdateProducts(ExcelWorksheet ws, int userId)
        {
            _logger.Log(LogLevel.Trace, "Reading GTIN's and SKU's from file...");

            var dictionary = new Dictionary<string, string>();
            var rowCount = ws.Dimension.End.Row;

            try
            {
                for (var row = 7; row <= rowCount; row++)
                {
                    var gtin = ws.Cells[row, 2].GetValue<string>();
                    var sku = ws.Cells[row, 4].GetValue<string>();
                    if (string.IsNullOrEmpty(gtin))
                        _logger.Log(LogLevel.Warn, $"Product GTIN is empty, SKU:{sku}");
                    else
                        dictionary.Add(sku, gtin);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "Error on read GTIN's and SKU's");
                throw new Exception("Error on read GTIN's and SKU's");
            }

            _logger.Log(LogLevel.Trace, "Getting products by SKU's...");
            var products = await _context.Products.Where(p => p.DeleteTime == null && dictionary.Keys.Contains(p.Sku)).ToListAsync();
            _logger.Log(LogLevel.Trace, "Products successfully received");

            _logger.Log(LogLevel.Trace, "Loading GTIN's attrValues...");
            var productIds = products.Select(p => p.Id);
            await _context.AttributeValues
                .Where(av => productIds.Contains(av.ProductId) && av.AttributeId == _exportAttributesIdsOptions.Gtin)
                .OrderByDescending(av => av.CreateTime)
                .LoadAsync();
            _logger.Log(LogLevel.Trace, "GTIN's attrValues successfully loaded");
            var nonRangeSizeProductsSkus = new List<string>();
            foreach (var product in products)
            {
                if (product.ModelLevel != ModelLevel.RangeSizeModel)
                {
                    nonRangeSizeProductsSkus.Add(product.Sku);
                    continue;
                }

                var gtinAttributeValue = product.AttributeValues.FirstOrDefault();
                if (gtinAttributeValue == null || gtinAttributeValue.StrValue != dictionary[product.Sku])
                {
                    product.AttributeValues.Add(new AttributeValue
                    {
                        ProductId = product.Id,
                        StrValue = dictionary[product.Sku],
                        CreateTime = DateTime.Now,
                        CreatorId = userId,
                        AttributeId = _exportAttributesIdsOptions.Gtin,
                        SearchString = dictionary[product.Sku]
                    });
                }
            }

            if (nonRangeSizeProductsSkus.Any())
            {
                var message = $"Trying to add GTIN attribute to non RangeSizeModel level product. SKU's [{string.Join(',', nonRangeSizeProductsSkus)}]";
                _logger.Log(LogLevel.Error, message);
                throw new Exception(message);
            }

            _logger.Log(LogLevel.Trace, "GTIN's added to products");
            _logger.Log(LogLevel.Trace, "Saving updated products to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, "Error on saving products");
                throw new Exception("Error on save products");
            }

            _logger.Log(LogLevel.Trace, "Products were successfully saved");

            _logger.Log(LogLevel.Trace, "Reading GTIN's and SKU's from file is finished");
        }

        [HttpGet("excel-template/{id}")]
        public async Task<IActionResult> GenerateImportExcelFileByCategoryId([FromRoute] int id)
        {
            _logger.Log(LogLevel.Info, $"Start generation import template file by category id: {id}...");
            var category = await _context.Categories
                .Include(c => c.AttributeCategories)
                .ThenInclude(ac => ac.Attribute)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                _logger.Log(LogLevel.Error, $"Category by id: {id} was not found.");
                return BadRequest($"Category by id: {id} was not found.");
            }

            byte[] fileContents;
            using (var pck = new ExcelPackage())
            {
                pck.Workbook.Worksheets.Add($"Модели");
                pck.Workbook.Worksheets.Add($"Цвето-модели");
                pck.Workbook.Worksheets.Add($"Размерные ряды");

                foreach (var ws in pck.Workbook.Worksheets)
                    foreach (var column in ExportExcelConfig.Headers)
                        ws.Cells[$"{column.Key}1"].Value = column.Value;

                var levels = Enum.GetValues(typeof(ModelLevel)).Cast<ModelLevel>().ToList();
                for (var i = 0; i < levels.Count; i++)
                {
                    var attributes = category.AttributeCategories.Where(ac => ac.ModelLevel == levels[i]).Select(ac => ac.Attribute).ToList();

                    for (var j = 0; j < attributes.Count; j++)
                        pck.Workbook.Worksheets[i].Cells[1, j + ExportExcelConfig.Headers.Count + 1].Value = attributes[j].Name;
                }

                foreach (var ws in pck.Workbook.Worksheets)
                    ws.Cells[1, 1, 1, ws.Dimension.End.Column].AutoFitColumns();

                fileContents = pck.GetACmpyteArray();
            }

            try
            {
                System.IO.File.WriteAllBytes($"{Path.GetTempPath()}//Шаблон импорта {category.Name}.xlsx", fileContents);
                _logger.Log(LogLevel.Info, $"Import template file was generated and saved to {Path.GetTempPath()}\\Шаблон импорта {category.Name}.xlsx");
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error in generation import template file. Exception: {e.Message}");
                return BadRequest($"Error in generation import template file. Exception: {e.Message}");
            }

            _logger.Log(LogLevel.Info, "Generation import template file by category id is finished");
            return Ok();
        }
    }
}