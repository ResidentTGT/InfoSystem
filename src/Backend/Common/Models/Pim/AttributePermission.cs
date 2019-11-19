
using Company.Common.Models.Identity;

namespace Company.Common.Models.Pim
{
    public class AttributePermission : DataPermissions
    {
        public int Id { get; set; }

        public int AttributeId { get; set; }

        public Attribute Attribute { get; set; }

        public override int RoleId { get; set; }

        public override DataAccessMethods Value { get; set; }
    }
}
