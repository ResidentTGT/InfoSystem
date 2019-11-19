using System;

namespace Company.Common.Models.Identity
{
    public class ResourcePermission
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ResourcePermissionsValues Value { get; set; }

        public int RoleId { get; set; }
    }

    [Flags]
    public enum ResourcePermissionsValues
    {
        None = 0,
        Read = 1,
        Modify = 2,
        Delete = 4,
        Add = 8
    }
}
