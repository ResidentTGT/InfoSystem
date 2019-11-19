
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Company.Common;
using Company.Pim.Client;
using Company.Common.Models.Pim;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Company.Pim.Client.v2;
using Company.Common.Enums;
using Company.Common.Extensions;

namespace Company.Pim.Parsing
{
    public class ImportParser
    {
        private const int _skuHeaderCellRowIndex = 1;
        private const int _skuHeaderCellColumnIndex = 1;
        private const int _syncIdHeaderCellColumnIndex = 2;
        private const int _parentIdHeaderCellColumnIndex = 3;

        private const int _nameHeaderCellColumnIndex = 4;
        private const int _categoryHeaderCellColumnIndex = 5;
        private const int _mainImageHeaderCellColumnIndex = 6;
        private const int _videosHeaderCellColumnIndex = 13;
        private const int _lastMetadataColumn = 18;

        private readonly ExcelWorker _excelWorker;

        private readonly PimContext _context;

        private readonly SkuGenerator _skuGenerator;

        private readonly Import _import;

        private readonly string _imagesPath;

        private readonly List<Common.Models.Pim.Attribute> _necessaryAttributes;

        private readonly IFileStorageMsCommunicator _httpFileStorageMsCommunicator;

        private List<Tuple<int, Common.Models.Pim.Attribute>> _attributes { get; set; } = new List<Tuple<int, Common.Models.Pim.Attribute>>();

        private Dictionary<string, Common.Models.Pim.Product> _productsDictionary { get; set; } = new Dictionary<string, Common.Models.Pim.Product>();

        private List<string> _productsErrors { get; set; }

        private List<Common.Models.Pim.Attribute> _allAttributes { get; set; }
        private List<Category> _allCategories { get; set; }
        private List<List> _allLists { get; set; }

        public ImportParser(PimContext context, Import import, SkuGenerator skuGenerator, IFileStorageMsCommunicator httpFileStorageMsCommunicator, string imagesPath, List<int> necessaryAttributes)
        {
            _context = context;
            _excelWorker = new ExcelWorker();
            _import = import;
            _skuGenerator = skuGenerator;
            _httpFileStorageMsCommunicator = httpFileStorageMsCommunicator;
            _imagesPath = imagesPath;
            _necessaryAttributes = necessaryAttributes != null
                ? _context.Attributes.Include(a => a.AttributeCategories).Where(a => necessaryAttributes.Contains(a.Id)).ToList()
                : new List<Common.Models.Pim.Attribute>();
            _productsErrors = new List<string>();
        }

        public void ParseFile(FileStream file)
        {
            using (var excel = new ExcelPackage(file))
            {
                var sheet = excel.Workbook.Worksheets.First();

                _allAttributes = _context.Attributes.Where(a => a.DeleteTime == null).ToList();
                _allCategories = _context.Categories.Where(a => a.DeleteTime == null).ToList();
                _allLists = _context.Lists.Where(a => a.DeleteTime == null).ToList();

                DoSheet(excel.Workbook.Worksheets[0], ModelLevel.Model);
                DoSheet(excel.Workbook.Worksheets[1], ModelLevel.ColorModel);
                DoSheet(excel.Workbook.Worksheets[2], ModelLevel.RangeSizeModel);

                var lastProductDbId = _context.Products.OrderBy(p => p.Id).Select(p => p.Id).LastOrDefault();

                var products = _productsDictionary.Where(p => p.Value.Sku == null).Select(p => p.Value).ToList();

                CheckModelConnectivity();         

                for (var i = 0; i < products.Count; i++)
                {
                    products[i].Sku = _skuGenerator.GenerateSku(lastProductDbId + i + 1);
                    _context.Products.Add(products[i]);
                }

                _import.Error = String.Join("#%_", _productsErrors);
            }
        }

        private void DoSheet(ExcelWorksheet sheet, ModelLevel modelLevel)
        {
            // TODO: Проверить столбцы заголовков

            // Подгружаем все атрибуты для данной страницы
            _attributes = GetAttributesForSheet(sheet);

            // Обрабатываем каждый товар с листа
            foreach (var cell in sheet.Cells[_skuHeaderCellRowIndex + 1, _nameHeaderCellColumnIndex, sheet.Dimension.End.Row, _nameHeaderCellColumnIndex])
            {
                // Проверяем заполнено ли значение ячейки
                if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    switch (modelLevel)
                    {
                        case ModelLevel.Model: _import.ModelCount += 1; break;
                        case ModelLevel.ColorModel: _import.ColorModelCount += 1; break;
                        case ModelLevel.RangeSizeModel: _import.RangeSizeModelCount += 1; break;
                    }

                    var skuCell = sheet.Cells[_excelWorker.GetRowIndexFromCell(cell), _skuHeaderCellColumnIndex];
                    var isEditableProduct = skuCell.Value != null && !String.IsNullOrWhiteSpace(skuCell.Value.ToString());

                    try
                    {
                        var productKeyValuePair = DoProductWithSyncId(sheet, skuCell, modelLevel, isEditableProduct);
                        _productsDictionary.Add(productKeyValuePair.Key, productKeyValuePair.Value);
                    }
                    catch (Exception e)
                    {
                        var errorMsg = isEditableProduct
                            ? $"Редактирование {skuCell.Value?.ToString()} (лист {sheet.Index + 1}, строка {_excelWorker.GetRowIndexFromCell(skuCell)}): {e.Message}"
                            : $"Создание (лист {sheet.Index + 1}, строка {_excelWorker.GetRowIndexFromCell(cell)}): {e.Message}";
                        _productsErrors.Add(errorMsg);
                    }
                }
            }
        }

        private List<Tuple<int, Common.Models.Pim.Attribute>> GetAttributesForSheet(ExcelWorksheet sheet)
        {
            var error = "В PIM не найдены следующие атрибуты: \n";
            var errorAttrs = new List<string>();
            var attributesOnSheet = new List<Tuple<int, Common.Models.Pim.Attribute>>();

            foreach (var cell in sheet.Cells[_skuHeaderCellRowIndex, _lastMetadataColumn + 1, _skuHeaderCellRowIndex, sheet.Dimension.End.Column])
            {
                if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    var attr = _allAttributes.FirstOrDefault(a => a.Name.Trim().ToUpper() == cell.Value.ToString().Trim().ToUpper());
                    if (attr == null)
                        errorAttrs.Add($"{cell.Value.ToString()} (лист {sheet.Index + 1 }, {cell.Address})");
                    else
                        attributesOnSheet.Add(new Tuple<int, Common.Models.Pim.Attribute>(_excelWorker.GetColumnIndexFromCell(cell), attr));
                }
            }

            if (errorAttrs.Count != 0)
            {
                error += String.Join(", ", errorAttrs);
                throw new Exception(error);
            }

            return attributesOnSheet;
        }

        private KeyValuePair<string, Product> DoProductWithSyncId(ExcelWorksheet sheet, ExcelRangeBase skuCell, ModelLevel modelLevel, bool isEditable)
        {
            var row = _excelWorker.GetRowIndexFromCell(skuCell);
            var errors = new List<string>();

            var parentIdValue = sheet.Cells[row, _parentIdHeaderCellColumnIndex].Value;

            var product = isEditable == true
                ? _context.Products.Include(p => p.ProductSearch).FirstOrDefault(p => p.DeleteTime == null && p.Sku == skuCell.Value.ToString())
                : new Product();

            var attrValues = new List<AttributeValue>();

            if (product == null)
            {
                errors.Add($"не найден товар по SKU '{skuCell.Value.ToString()}' ({skuCell.Address})");
            }
            else
            {
                if (parentIdValue == null)
                {
                    if (sheet.Index > 0)
                    {
                        errors.Add($"не заполнено значение parentId");
                    }
                }
                else
                {
                    if (!_productsDictionary.ContainsKey($"{sheet.Index - 1}-{parentIdValue}"))
                    {
                        errors.Add($"на листе '{sheet.Index}' нету товара с syncId '{parentIdValue.ToString()}'");
                    }
                    else
                    {
                        product.ParentProduct = _productsDictionary[$"{sheet.Index - 1}-{parentIdValue}"];
                    }
                }

                var nameCell = sheet.Cells[row, _nameHeaderCellColumnIndex];
                if (nameCell.Value != null && !String.IsNullOrWhiteSpace(nameCell.Value.ToString()))
                    product.Name = nameCell.Value.ToString();
                else
                    errors.Add($"пустое наименование товара ({nameCell.Address})");

                var categoryCell = sheet.Cells[row, _categoryHeaderCellColumnIndex];
                if (categoryCell.Value != null && !String.IsNullOrWhiteSpace(categoryCell.Value.ToString()))
                {
                    var categoryId = FindCategory(categoryCell.Value.ToString());
                    if (categoryId == null)
                        errors.Add($"категория '{categoryCell.Value.ToString()}' не найдена ({categoryCell.Address})");
                    else
                        product.CategoryId = categoryId;
                }
                else
                    product.CategoryId = null;

                product.ModelLevel = modelLevel;
                attrValues = DoProperties(sheet, skuCell, errors, product, isEditable);
            }

            var syncIdValue = sheet.Cells[row, _syncIdHeaderCellColumnIndex].Value;
            if (syncIdValue == null)
            {
                errors.Add($"не заполнено значение syncId");
            }

            if (errors.Count == 0)
            {
                product.ProductFiles = new List<ProductFile>();

                FillMainImageCell(sheet, row, product, errors);

                product.ImportId = _import.Id;
                product.CreateTime = DateTime.Now;
                product.CreatorId = _import.CreatorId;

                switch (modelLevel)
                {
                    case ModelLevel.Model: _import.ModelSuccessCount += 1; break;
                    case ModelLevel.ColorModel: _import.ColorModelSuccessCount += 1; break;
                    case ModelLevel.RangeSizeModel: _import.RangeSizeModelSuccessCount += 1; break;
                }
                product.AttributeValues.AddRange(attrValues);
            }
            else
            {
                _import.ErrorCount += 1;
                throw new Exception(String.Join(", ", errors));
            }

            return new KeyValuePair<string, Product>($"{sheet.Index}-{syncIdValue}", product);
        }

        private void FillMainImageCell(ExcelWorksheet sheet, int row, Product product, List<string> errors)
        {
            var mainImageCell = sheet.Cells[row, _mainImageHeaderCellColumnIndex];
            if (mainImageCell.Value != null && !String.IsNullOrWhiteSpace(mainImageCell.Value.ToString()))
            {
                try
                {
                    FileInfo[] filesInDir = new DirectoryInfo(_imagesPath).GetFiles(mainImageCell.Value.ToString());
                    if (filesInDir.Length == 0)
                        throw new Exception($"не найдено изображение с именем {mainImageCell.Value.ToString()}");

                    var file = _httpFileStorageMsCommunicator.SaveImage(File.ReadAllBytes(Path.Combine(_imagesPath, mainImageCell.Value.ToString())), mainImageCell.Value.ToString());
                    product.ProductFiles.Add(new ProductFile()
                    {
                        FileId = file.Id,
                        FileType = FileType.Image,
                        IsMain = true,
                    });

                }
                catch (Exception e)
                {
                    errors.Add($"{e.Message} ({mainImageCell.Address})");
                }
            }
        }

        private int? FindCategory(string categoryStr)
        {
            var cats = categoryStr.Split('$');

            var categories = new List<Category>();
            for (var i = 0; i < cats.Length; i++)
            {
                var category = i == 0
                    ? _allCategories.FirstOrDefault(c => c.Name.ToUpper().Trim() == cats[i].ToUpper().Trim() && c.ParentId == null)
                    : _allCategories.FirstOrDefault(c => c.Name.ToUpper().Trim() == cats[i].ToUpper().Trim() && c.ParentId == categories[i - 1].Id);
                if (category == null)
                    return null;
                else
                    categories.Add(category);
            }

            return categories.Last().Id;
        }

        private List<AttributeValue> DoProperties(ExcelWorksheet sheet, ExcelRangeBase skuCell, List<string> errors, Product product, bool isEditable)
        {
            var row = _excelWorker.GetRowIndexFromCell(skuCell);
            var attrValues = new List<AttributeValue>();

            for (var i = 0; i < _attributes.Count; i++)
            {
                var cell = sheet.Cells[row, _attributes[i].Item1];

                if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    AttributeValue attrValue = null;
                    if (isEditable)
                        attrValue = _context.AttributeValues
                            .Include(av => av.Attribute)
                            .OrderByDescending(ac => ac.CreateTime)
                            .FirstOrDefault(av => av.AttributeId == _attributes[i].Item2.Id && av.ProductId == product.Id);
                    if (isEditable && attrValue != null && !IsAttributeValueChanged(cell.Value.ToString(), attrValue))
                        continue;
                    else
                        attrValue = new AttributeValue();

                    var cellValue = cell.Value.ToString().Trim();
                    switch (_attributes[i].Item2.Type)
                    {
                        case AttributeType.Boolean:

                            if (cellValue.ToUpper() == "ДА" || cellValue.ToUpper() == "НЕТ")
                                attrValue.BoolValue = cellValue.ToUpper() == "ДА" ? true : false;
                            else
                            {
                                errors.Add($"значение атрибута должно быть ДА или НЕТ ({cell.Address})");
                                continue;
                            }
                            break;
                        case AttributeType.String:
                            attrValue.StrValue = cellValue;
                            break;
                        case AttributeType.Text:
                            attrValue.StrValue = cellValue;
                            break;
                        case AttributeType.Number:
                            if (Double.TryParse(cellValue.Replace(".", ","), out double res))
                                attrValue.NumValue = res;
                            else
                            {
                                errors.Add($"значение атрибута должно быть числом ({cell.Address})");
                                continue;
                            }
                            break;
                        case AttributeType.Date:
                            if (DateTime.TryParse(cellValue, out DateTime date))
                                attrValue.DateValue = DateTime.Parse(cellValue);
                            else
                            {
                                errors.Add($"значение атрибута должно быть датой ({cell.Address})");
                                continue;
                            }
                            break;
                        case AttributeType.List:
                            var listValue = _context.ListValues.FirstOrDefault(lv => lv.DeleteTime == null && lv.ListId == _attributes[i].Item2.ListId && lv.Value.ToUpper().Trim() == cellValue.ToUpper());
                            var list = _allLists.FirstOrDefault(l => l.Id == _attributes[i].Item2.ListId);
                            if (listValue != null)
                                attrValue.ListValueId = listValue.Id;
                            else
                            {
                                errors.Add($"для списка {list.Name} нет значения {cellValue} ({cell.Address})");
                                continue;
                            }
                            break;
                    }
                    if (isEditable)
                        attrValue.ProductId = product.Id;

                    var lValue = _context.ListValues.Where(lv => lv.DeleteTime == null && lv.Id == attrValue.ListValueId)?.FirstOrDefault();
                    attrValue.SearchString = attrValue.StrValue + attrValue.NumValue?.ToString() + lValue?.Value + (attrValue.BoolValue != null ? ((bool)attrValue.BoolValue ? "Да" : "Нет") : null);
                    attrValue.AttributeId = _attributes[i].Item2.Id;
                    attrValue.CreateTime = DateTime.Now;
                    attrValue.CreatorId = _import.CreatorId;
                    attrValues.Add(attrValue);
                }
            }

            if (!isEditable)
            {
                foreach (var attribute in _necessaryAttributes.Where(a => a.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId && ac.ModelLevel == product.ModelLevel)))
                    if (!attrValues.Any(av => av.AttributeId == attribute.Id))
                        errors.Add($"для обязательного атрибута \"{ attribute.Name}\" не установлено значение.");
            }
            else
            {
                var necessaryAttributes = _necessaryAttributes.Where(a => a.AttributeCategories.Any(ac => ac.CategoryId == product.CategoryId && ac.ModelLevel == product.ModelLevel));
                var originAttrValues = product.AttributeValues.Where(av => necessaryAttributes.Any(a => a.Id == av.AttributeId))
                                                              .GroupBy(a => new { a.AttributeId })
                                                              .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                                                              .ToList();

                originAttrValues.AddRange(attrValues.Where(av => necessaryAttributes.Any(a => a.Id == av.AttributeId)));
                originAttrValues = originAttrValues.GroupBy(a => new { a.AttributeId })
                                .Select(g => g.OrderByDescending(av => av.CreateTime).First())
                                .ToList();

                foreach (var attribute in necessaryAttributes)
                {
                    var attrValue = originAttrValues.FirstOrDefault(av => av.AttributeId == attribute.Id);
                    if (attrValue == null || (attrValue.BoolValue == null && attrValue.DateValue == null & attrValue.ListValueId == null && attrValue.NumValue == null && attrValue.StrValue == null))
                        errors.Add($"для обязательного атрибута \"{ attribute.Name}\" не установлено значение.");
                }

            }

            return attrValues;
        }

        private bool IsAttributeValueChanged(string value, AttributeValue attrValue)
        {
            switch (attrValue.Attribute.Type)
            {
                case AttributeType.Boolean:
                    if (value.ToUpper() == "ДА" || value.ToUpper() == "НЕТ")
                    {
                        var b = value.ToUpper() == "ДА" ? true : false;
                        if (b == attrValue.BoolValue)
                            return false;
                    }
                    break;
                case AttributeType.Date:
                    if (DateTime.TryParse(value, out DateTime date) && date == attrValue.DateValue)
                        return false;
                    break;
                case AttributeType.Number:
                    if (Double.TryParse(value, out double res) && attrValue.NumValue == res)
                        return false;
                    break;
                case AttributeType.String:
                    if (value == attrValue.StrValue)
                        return false;
                    break;
                case AttributeType.Text:
                    if (value == attrValue.StrValue)
                        return false;
                    break;
                case AttributeType.List:
                    var listValue = _context.ListValues.FirstOrDefault(lv => attrValue.ListValueId == lv.Id);
                    if (listValue?.Value == value)
                        return false;
                    break;
            }

            return true;
        }

        private void CheckModelConnectivity()
        {
            foreach (var item in _productsDictionary.Where(v => v.Value.ModelLevel == ModelLevel.Model))
            {
                var colorModels = _productsDictionary.Where(v => v.Value.ModelLevel == ModelLevel.ColorModel).Select(cm=>cm.Value.ParentProduct).ToList();
                if (!colorModels.Contains(item.Value))
                {
                    _import.ModelSuccessCount--;
                    _productsErrors.Add($"Для модели с наименованием '{item.Value.Name}' не найдено цвето-моделей.");
                }
            }

            foreach (var item in _productsDictionary.Where(v => v.Value.ModelLevel == ModelLevel.ColorModel))
            {
                var sizeModels = _productsDictionary.Where(v => v.Value.ModelLevel == ModelLevel.RangeSizeModel).Select(cm => cm.Value.ParentProduct).ToList();
                if (!sizeModels.Contains(item.Value))
                {
                    _import.ColorModelSuccessCount--;
                    _productsErrors.Add($"Для цвето-модели с наименованием '{item.Value.Name}' не найдено размерных рядов.");
                }
            }
        }

    }
}
