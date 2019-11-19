using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ProductProperties
    {
        public AttributeDto Attribute { get; set; }

        public AttributeValueDto AttributeValue { get; set; }

        public ProductProperties()
        {
        }

        public ProductProperties(AttributeValue attributeValue)
        {
            AttributeValue = new AttributeValueDto(attributeValue);

            Attribute = attributeValue.Attribute != null
                ? new AttributeDto(attributeValue.Attribute)
                : null;
        }

        public AttributeValue ToEntity()
        {
            return new AttributeValue()
            {
                Id = AttributeValue.Id,
                ListValueId = AttributeValue.ListValueId,
                StrValue = AttributeValue.StrValue,
                NumValue = AttributeValue.NumValue,
                BoolValue = AttributeValue.BoolValue,
                DateValue = AttributeValue.DateValue,
                AttributeId = AttributeValue.AttributeId,
                ProductId = AttributeValue.ProductId,
                Attribute = Attribute?.ToEntity()
            };
        }
    }
}