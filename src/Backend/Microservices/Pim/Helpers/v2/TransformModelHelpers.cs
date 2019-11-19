using Company.Common.Models.Pim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pim.Helpers.v2
{
    public class TransformModelHelpers
    {
        public Product TransformProduct(Product product, bool withSubProducts = true, bool withParents = true)
            => new Product()
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                CreateTime = product.CreateTime,
                CreatorId = product.CreatorId,
                DeleteTime = product.DeleteTime,
                DeleterId = product.DeleterId,
                ParentId = product.ParentId,
                ModelLevel = product.ModelLevel,
                ParentProduct = withParents
                ? (product.ParentProduct == null ? null : TransformProduct(product.ParentProduct, false))
                : null,

                SubProducts = withSubProducts
                    ? product.SubProducts.Where(sc => sc.DeleteTime == null).Select(sc => TransformProduct(sc, true)).ToList()
                    : new List<Product>(),

                ProductFiles = product.ProductFiles.Select(pf => TransformProductFile(pf)).ToList(),

                AttributeValues = product.AttributeValues.GroupBy(a => a.AttributeId)
                    .Select(g => g.OrderByDescending(av => av.CreateTime).First()).Select(av => TransformAttributeValue(av)).ToList()
            };


        public ProductFile TransformProductFile(ProductFile productFile)
            => new ProductFile()
            {
                IsMain = productFile.IsMain,
                FileType = productFile.FileType,
                FileId = productFile.FileId,
            };

        public AttributeValue TransformAttributeValue(AttributeValue attributeValue)
            => new AttributeValue()
            {
                Id = attributeValue.Id,
                ListValueId = attributeValue.ListValueId,
                StrValue = attributeValue.StrValue,
                NumValue = attributeValue.NumValue,
                BoolValue = attributeValue.BoolValue,
                DateValue = attributeValue.DateValue,
                AttributeId = attributeValue.AttributeId,
                ProductId = attributeValue.ProductId,
                Attribute = attributeValue.Attribute == null ? null : TransformAttribute(attributeValue.Attribute)
            };

        public Category TransformCategory(Category category, bool withSubcategories = false)
            => new Category()
            {
                Id = category.Id,
                ParentId = category.ParentId,
                Name = category.Name,

                SubCategories = withSubcategories
                                        ? category.SubCategories.Where(sc => sc.DeleteTime == null).Select(sc => TransformCategory(sc, true)).ToList()
                                        : new List<Category>()
            };

        public AttributeCategory TransformAttributeCategory(AttributeCategory attributeCategory)
            => new AttributeCategory()
            {
                AttributeId = attributeCategory.AttributeId,
                CategoryId = attributeCategory.CategoryId,
                ModelLevel = attributeCategory.ModelLevel,
                IsRequired = attributeCategory.IsRequired,
                IsFiltered = attributeCategory.IsFiltered,
                IsVisibleInProductCard = attributeCategory.IsVisibleInProductCard,
                IsKey = attributeCategory.IsKey,
            };

        public Common.Models.Pim.Attribute TransformAttribute(Common.Models.Pim.Attribute attribute, bool withCategories = false, bool withAttributePermissions = true)
            => new Common.Models.Pim.Attribute()
            {
                Id = attribute.Id,
                Name = attribute.Name,
                Type = attribute.Type,
                GroupId = attribute.GroupId,
                ListId = attribute.ListId,
                Template = attribute.Template,
                MaxLength = attribute.MaxLength,
                MinLength = attribute.MinLength,
                Max = attribute.Max,
                Min = attribute.Min,
                MinDate = attribute.MinDate,
                MaxDate = attribute.MaxDate,
                AttributePermissions = withAttributePermissions
                ? attribute.AttributePermissions.Select(ap => new AttributePermission()
                {
                    Id = ap.Id,
                    RoleId = ap.RoleId,
                    Value = ap.Value,
                    AttributeId = attribute.Id
                }).ToList()
                : new List<AttributePermission>(),
                AttributeCategories = withCategories
                                        ? attribute.AttributeCategories.Select(ac => new AttributeCategory() { CategoryId = ac.CategoryId }).ToList()
                                        : new List<AttributeCategory>()
            };

        public List TransformList(List list, bool withListMetadatas = true, bool withListValues = true) => new List()
        {
            Id = list.Id,
            Name = list.Name,
            Template = list.Template,
            ListValues = list.ListValues.Select(lv => TransformListValue(lv)).ToList(),
            ListMetadatas = list.ListMetadatas.Select(
                lm => new ListMetadata()
                {
                    Id = lm.Id,
                    ListId = lm.ListId,
                    Name = lm.Name
                }).ToList(),
            CreatorId = list.CreatorId
        };


        public ListValue TransformListValue(ListValue listValue, bool withListMetadatas = true, bool withListValueMetadatas = true) => new ListValue()
        {
            Id = listValue.Id,
            Value = listValue.Value,
            ListId = listValue.ListId,
            ListValueMetadatas = withListValueMetadatas ? listValue.ListValueMetadatas.Select(lv => TransformListValueMetadata(lv, withListMetadatas)).ToList() : null
        };

        public ListValueMetadata TransformListValueMetadata(ListValueMetadata listValueMetadata, bool withListMetadatas = true) => new ListValueMetadata()
        {
            Id = listValueMetadata.Id,
            ListMetadataId = listValueMetadata.ListMetadataId,
            ListValueId = listValueMetadata.ListValueId,
            Value = listValueMetadata.Value,
            ListMetadata = withListMetadatas ? new ListMetadata()
            {
                Id = listValueMetadata.ListMetadata.Id,
                ListId = listValueMetadata.ListMetadata.ListId,
                Name = listValueMetadata.ListMetadata.Name,
            } : null
        };
    }
}