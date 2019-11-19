using System;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Calculator
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

        public AttributeValue ToEntity()
        {
            return new AttributeValue()
            {
                Id = Id,
                ListValueId = ListValueId,
                StrValue = StrValue,
                NumValue = NumValue,
                BoolValue = BoolValue,
                DateValue = DateValue,
                AttributeId = AttributeId,
                ProductId = ProductId,
            };
        }
    }
}