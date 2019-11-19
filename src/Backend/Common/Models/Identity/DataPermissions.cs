using System;

namespace Company.Common.Models.Identity
{
    public abstract class DataPermissions
    {
        public abstract int RoleId { get; set; }
        
        // TODO Should It be abstract??
        public abstract DataAccessMethods Value { get; set; }
    }

    [Flags]
    public enum DataAccessMethods
    {
        Read = 1,
        Write = 2
    }
}
