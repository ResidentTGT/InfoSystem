using Company.Common.Models.Identity;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class AttributePermissionDto
    {
        public int Id { get; set; }

        public int AttributeId { get; set; }

        public int RoleId { get; set; }

        public DataAccessMethods Value { get; set; }

        public AttributePermissionDto() { }

        public AttributePermissionDto(AttributePermission attributePermission)
        {
            AttributeId = attributePermission.AttributeId;
            RoleId = attributePermission.RoleId;
            Value = attributePermission.Value;
            Id = attributePermission.Id;
        }

        public AttributePermission ToEntity()
        {
            return new AttributePermission()
            {
                Id = Id,
                AttributeId = AttributeId,
                RoleId = RoleId,
                Value = Value,
            };
        }
    }
}