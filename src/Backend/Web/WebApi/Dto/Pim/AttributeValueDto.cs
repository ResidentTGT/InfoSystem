using System;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class AttributeValueDto
    {
        public int Id { get; set; }

        public int? ListValueId { get; set; }

        public string StrValue { get; set; }

        public double? NumValue { get; set; }

        public bool? BoolValue { get; set; }

        public DateTime? DateValue { get; set; }

        public int AttributeId { get; set; }

        public int ProductId { get; set; }

        public AttributeValueDto()
        {
        }

        public AttributeValueDto(AttributeValue attributeValue)
        {
            Id = attributeValue.Id;
            ListValueId = attributeValue.ListValueId;
            StrValue = attributeValue.StrValue;
            NumValue = attributeValue.NumValue;
            BoolValue = attributeValue.BoolValue;
            DateValue = attributeValue.DateValue;
            AttributeId = attributeValue.AttributeId;
            ProductId = attributeValue.ProductId;
        }
    }
}