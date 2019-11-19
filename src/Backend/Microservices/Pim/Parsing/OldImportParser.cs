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

namespace Company.Pim.Parsing
{
    public class OldImportParser
    {
        private const int _skuHeaderCellRowIndex = 1;
        private const int _skuHeaderCellColumnIndex = 1;
        private const int _nameHeaderCellColumnIndex = 2;
        private const int _categoryHeaderCellColumnIndex = 3;
        private const int _mainImageHeaderCellColumnIndex = 4;
        private const int _imagesHeaderCellColumnIndex = 5;
        private const int _videosHeaderCellColumnIndex = 6;
        private const int _documentsHeaderCellColumnIndex = 7;

        private readonly ExcelWorker _excelWorker;

        private readonly PimContext _context;

        private readonly SkuGenerator _skuGenerator;

        private readonly Import _import;

        private readonly string _imagesPath;

        private readonly IFileStorageMsCommunicator _httpFileStorageMsCommunicator;

        private List<Tuple<int, Common.Models.Pim.Attribute>> _attributes { get; set; } = new List<Tuple<int, Common.Models.Pim.Attribute>>();

        private List<string> _productsErrors { get; set; }

        private List<Common.Models.Pim.Attribute> _allAttributes { get; set; }
        private List<Category> _allCategories { get; set; }
        private List<List> _allLists { get; set; }

        public OldImportParser(PimContext context, Import import, SkuGenerator skuGenerator, IFileStorageMsCommunicator httpFileStorageMsCommunicator, string imagesPath)
        {
            _context = context;
            _excelWorker = new ExcelWorker();
            _import = import;
            _skuGenerator = skuGenerator;
            _httpFileStorageMsCommunicator = httpFileStorageMsCommunicator;
            _imagesPath = imagesPath;
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

                LoadAttributes(sheet);

                var products = new List<Product>();
                foreach (var cell in sheet.Cells[_skuHeaderCellRowIndex + 1, _nameHeaderCellColumnIndex, sheet.Dimension.End.Row, _nameHeaderCellColumnIndex])
                {
                    if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                    {
                        _import.RangeSizeModelCount += 1;
                        var skuCell = sheet.Cells[_excelWorker.GetRowIndexFromCell(cell), _skuHeaderCellColumnIndex];
                        if (skuCell.Value != null && !String.IsNullOrWhiteSpace(skuCell.Value.ToString()))
                            try
                            {
                                DoProduct(sheet, skuCell, true);
                            }
                            catch (Exception e)
                            {
                                _productsErrors.Add($"Редактирование {skuCell.Value.ToString()} (строка {_excelWorker.GetRowIndexFromCell(skuCell)}): {e.Message}");
                            }

                        else
                        {
                            try
                            {
                                products.Add(DoProduct(sheet, skuCell, false));
                            }
                            catch (Exception e)
                            {
                                _productsErrors.Add($"Создание (строка {_excelWorker.GetRowIndexFromCell(cell)}): {e.Message}");
                            }
                        }
                    }
                }

                var lastProduct = _context.Products.OrderBy(p => p.Id).LastOrDefault();
                int lastDbId = lastProduct == null ? 0 : lastProduct.Id;
                for (var i = 0; i < products.Count; i++)
                {
                    products[i].Sku = _skuGenerator.GenerateSku(lastDbId + i + 1);
                    _context.Products.Add(products[i]);
                }

                _import.Error = String.Join("#%_", _productsErrors);

            }
        }

        private void LoadAttributes(ExcelWorksheet sheet)
        {
            var error = "В PIM не найдены следующие атрибуты: \n";
            var errorAttrs = new List<string>();

            foreach (var cell in sheet.Cells[_skuHeaderCellRowIndex, _documentsHeaderCellColumnIndex + 1, _skuHeaderCellRowIndex, sheet.Dimension.End.Column])
            {
                if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    var attr = _allAttributes.FirstOrDefault(a => a.Name.Trim().ToUpper() == cell.Value.ToString().Trim().ToUpper());
                    if (attr == null)
                        errorAttrs.Add($"{cell.Value.ToString()} ({cell.Address})");
                    else
                        _attributes.Add(new Tuple<int, Common.Models.Pim.Attribute>(_excelWorker.GetColumnIndexFromCell(cell), attr));
                }
            }

            if (errorAttrs.Count != 0)
            {
                error += String.Join(", ", errorAttrs);
                throw new Exception(error);
            }
        }

        private Product DoProduct(ExcelWorksheet sheet, ExcelRangeBase skuCell, bool isEditable)
        {
            var row = _excelWorker.GetRowIndexFromCell(skuCell);
            var errors = new List<string>();

            var product = isEditable == true
                ? _context.Products.FirstOrDefault(p => p.DeleteTime == null && p.Sku == skuCell.Value.ToString())
                : new Product();

            var attrValues = new List<AttributeValue>();

            if (product == null)
                errors.Add($"не найдено товара по SKU {skuCell.Value.ToString()} ({skuCell.Address})");
            else
            {
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

                var productProp = isEditable ? product : null;
                attrValues = DoProperties(sheet, skuCell, errors, productProp);
            }

            product.ProductFiles = new List<ProductFile>();

            if (errors.Count == 0)
            {
                FillMainImageCell(sheet, row, product, errors);

                product.ImportId = _import.Id;
                product.CreateTime = DateTime.Now;
                product.CreatorId = _import.CreatorId;
                product.ModelLevel = Common.Enums.ModelLevel.RangeSizeModel;
                _import.RangeSizeModelSuccessCount += 1;
                if (!isEditable)
                    product.AttributeValues = attrValues;
                else
                    foreach (var attr in attrValues)
                        product.AttributeValues.Add(attr);

                return isEditable ? null : product;
            }
            else
            {
                _import.ErrorCount += 1;
                throw new Exception(String.Join(", ", errors));
            }
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

        private List<AttributeValue> DoProperties(ExcelWorksheet sheet, ExcelRangeBase skuCell, List<string> errors, Product product)
        {
            var row = _excelWorker.GetRowIndexFromCell(skuCell);
            var attrValues = new List<AttributeValue>();

            for (var i = 0; i < _attributes.Count; i++)
            {
                var cell = sheet.Cells[row, _attributes[i].Item1];

                if (cell.Value != null && !String.IsNullOrWhiteSpace(cell.Value.ToString()))
                {
                    AttributeValue attrValue = null;
                    if (product != null)
                        attrValue = _context.AttributeValues.Include(av => av.Attribute).OrderByDescending(ac => ac.CreateTime).FirstOrDefault(av => av.AttributeId == _attributes[i].Item2.Id && av.ProductId == product.Id);
                    if (product != null && attrValue != null && !IsAttributeValueChanged(cell.Value.ToString(), attrValue))
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
                    if (product != null)
                        attrValue.ProductId = product.Id;

                    var lValue = _context.ListValues.Where(lv => lv.DeleteTime == null && lv.Id == attrValue.ListValueId)?.FirstOrDefault();
                    attrValue.SearchString = attrValue.StrValue + attrValue.NumValue?.ToString() + lValue?.Value + (attrValue.BoolValue != null ? ((bool)attrValue.BoolValue ? "Да" : "Нет") : null);
                    attrValue.AttributeId = _attributes[i].Item2.Id;
                    attrValue.CreateTime = DateTime.Now;
                    attrValue.CreatorId = _import.CreatorId;
                    attrValues.Add(attrValue);
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
                    if (listValue.Value == value)
                        return false;
                    break;
            }

            return true;
        }

    }
}
