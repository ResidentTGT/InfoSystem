using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;
using Web1cApi.Communicators;
using Web1cApi.Dto;

namespace Web1cApi.Helpers
{
    public class TransformModelHelper : ITransformModelHelper
    {
        public List<Product1cDto> PopulateProduct1cDto(List<TradeManagementResponseDto> ltmr, Dictionary<int, string> lvDictionary)
        {
            var productsDto = new List<Product1cDto>();
            foreach (var tmr in ltmr)
            {
                productsDto.Add(new Product1cDto()
                {
                    Name = tmr.Product.Name,
                    Sku = tmr.Product.Sku,
                    CategoryName = tmr.CategoryTreeString,
                    Properties = tmr.Product.AttributeValues.Select(av => PopulatePropertyDto(av, lvDictionary)).ToList()
                });
            }

            return productsDto;
        }

        private PropertyDto PopulatePropertyDto(AttributeValue attributeValue, Dictionary<int, string> lvDictionary)
        {
            var property = new PropertyDto()
            {
                Id = attributeValue.AttributeId,
                AttributeName = attributeValue.Attribute.Name
            };

            switch (attributeValue.Attribute.Type)
            {
                case AttributeType.Boolean:
                    property.BoolValue = attributeValue.BoolValue;
                    break;
                case AttributeType.List:
                    property.ListValue = attributeValue.ListValueId != null ? lvDictionary[attributeValue.ListValueId.Value] : null;
                    break;
                case AttributeType.Number:
                    property.NumValue = attributeValue.NumValue;
                    break;
                case AttributeType.String:
                    property.StrValue = attributeValue.StrValue;
                    break;
                case AttributeType.Text:
                    property.StrValue = attributeValue.StrValue;
                    break;
                case AttributeType.Date:
                    property.DateValue = attributeValue.DateValue;
                    break;
                default:
                    throw new ArgumentException("Wrong type of attribute.");
            }

            return property;
        }


    }
}