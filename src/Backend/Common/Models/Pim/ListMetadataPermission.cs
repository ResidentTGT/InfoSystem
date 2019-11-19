
using Company.Common.Models.Identity;

namespace Company.Common.Models.Pim
{
    public class ListMetadataPermission : DataPermissions
    {
        public int Id { get; set; }

        public int ListMetadataId { get; set; }

        public virtual ListMetadata ListMetadata { get; set; }

        public override int RoleId { get; set; }

        public override DataAccessMethods Value { get; set; }
    }
}
