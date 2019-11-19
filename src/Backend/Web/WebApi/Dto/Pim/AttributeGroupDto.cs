using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class AttributeGroupDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public AttributeGroupDto()
        {
        }

        public AttributeGroupDto(AttributeGroup attributeGroup)
        {
            Id = attributeGroup.Id;
            Name = attributeGroup.Name;
        }

        public AttributeGroup ToEntity()
        {
            return new AttributeGroup()
            {
                Id = Id,
                Name = Name
            };
        }
    }
}