using System;
using System.Collections.Generic;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Company.Common.Enums;
using System.Text.RegularExpressions;

namespace Company.Common.Models.Pim
{
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Sku { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? ParentId { get; set; }

        public int? CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public int? CategoryId { get; set; }

        /// <summary>
        /// Cancelled by fabric
        /// </summary>
        public bool Canceled { get; set; }

        public virtual Product ParentProduct { get; set; }

        public Category Category { get; set; }

        public ProductSearch ProductSearch { get; set; }

        public int? ImportId { get; set; }

        public ModelLevel ModelLevel { get; set; }

        public virtual Import Import { get; set; }

        public ICollection<Product> SubProducts { get; set; } = new List<Product>();

        public ICollection<ProductFile> ProductFiles { get; set; } = new List<ProductFile>();

        public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();

        public Product()
        {

        }

        public void UpdateSearchStringArray()
        {
            if (ProductSearch == null)
            {
                ProductSearch = new ProductSearch();
            }

            foreach (var subProduct in SubProducts)
            {
                if (subProduct.ProductSearch == null)
                {
                    subProduct.ProductSearch = new ProductSearch()
                    {
                        ParentProductSearch = ProductSearch
                    };
                }
            }

            // Initialization of full text search string with format "AttributeName:AttributeValue"
            if (Category == null)
            {
                ProductSearch.FullSearchArray = AttributeValues
                                            .Where(ac => ac.Attribute != null && ac.Attribute.DeleteTime == null)
                                            .Select(ac => $"{ac.Attribute.Name}:{AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == ac.AttributeId)?.SearchString ?? "NULL"}".ToUpper())
                                            .ToList();
            }
            else
            {
                ProductSearch.FullSearchArray = AttributeValues//Category?.AttributeCategories
                        .Where(ac => ac.Attribute != null && ac.Attribute.DeleteTime == null)
                        .Select(ac => $"{ac.Attribute.Name}:{AttributeValues.FirstOrDefault(av => av.AttributeId == ac.AttributeId)?.SearchString ?? "NULL"}".ToUpper())
                        .ToList();
            }


            // Initialization of fast full text search string, which will be only with Attribute values search strings 
            // TODO: Move it into config file
            var attributeIds = new List<int> { 125, 271, 121, 77, 300, 580, 241, 266 };

            ProductSearch.SmartSearchArray = AttributeValues.Where(av => attributeIds.Contains(av.AttributeId)).Select(av => av.SearchString?.ToUpper()).ToList();

            ProductSearch.SmartSearchArray.Add(Name.ToUpper());
            ProductSearch.SmartSearchArray.Add(Sku.ToUpper());

            ProductSearch.ImportId = ImportId;
            ProductSearch.CategoryId = CategoryId;

            if (ModelLevel == ModelLevel.Model)
            {
                ProductSearch.NameOrigEng = AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 238)?.StrValue;
                ProductSearch.BwpMin = SubProducts != null && SubProducts.Count > 0 ? SubProducts.Min(
                                            sp => sp.SubProducts.Any() ? sp.SubProducts.Min(
                                                spp => spp.AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 177)?.NumValue ?? 0
                                            ) : 0
                                        )
                                        : 0;
            }

            if (ModelLevel == ModelLevel.RangeSizeModel)
            {
                var ageMonthFrom = GetFloatListValueValue(AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 330)?.SearchString);
                var ageMonthTo = GetFloatListValueValue(AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 347)?.SearchString);
                var ageYearFrom = GetFloatListValueValue(AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 299)?.SearchString);
                var ageYearTo = GetFloatListValueValue(AttributeValues.OrderBy(av => av.CreateTime).FirstOrDefault(av => av.AttributeId == 346)?.SearchString);

                ProductSearch.AgeMonthRange = GenerateNpgsqlRange((int?)ageMonthFrom, (int?)ageMonthTo);
                ProductSearch.AgeYearRange = GenerateNpgsqlRange((int?)ageYearFrom, (int?)ageYearTo);
            }
            else
            {
                ProductSearch.AgeMonthRange = GenerateNpgsqlRange(null, null);
                ProductSearch.AgeYearRange = GenerateNpgsqlRange(null, null);
            }
        }

        public void UpdateSearchStringByOneAttribute(AttributeValue attributeValue)
        {
            if (attributeValue.Attribute == null)
                return;

            if (ProductSearch == null)
                ProductSearch = new ProductSearch();

            var oldAttributeValue = ProductSearch.FullSearchArray.FirstOrDefault(s => s.Contains(attributeValue.Attribute.Name.ToUpper()));
            if (oldAttributeValue != null)
            {
                ProductSearch.FullSearchArray.Remove(oldAttributeValue);
            }

            ProductSearch.FullSearchArray.Add($"{attributeValue.Attribute.Name}:{attributeValue.SearchString ?? "NULL"}".ToUpper());
        }

        private NpgsqlTypes.NpgsqlRange<int> GenerateNpgsqlRange(int? from, int? to)
            => new NpgsqlTypes.NpgsqlRange<int>(from.GetValueOrDefault(), true, !from.HasValue, to.GetValueOrDefault() + 1, true, !to.HasValue);

        private float? GetFloatListValueValue(string value)
            => float.TryParse(value, out float res) ? (float?)res : null;
    }
}