using Company.Common.Models.Identity;

namespace Company.Common.Models.Users
{
    public class SectionPermission
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int RoleId { get; set; }
        public virtual Role Role { get; set; }
    }
}
