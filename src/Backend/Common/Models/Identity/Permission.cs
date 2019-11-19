using System;

namespace Company.Common.Models.Identity
{
    public class Permission
    {
        public int Id { get; set; }

        public int ExternalApplicationId { get; set; }
        public virtual ExternalApplication ExternalApplication { get; set; }

        public string Name { get; set; }

        public Methods Methods { get; set; }
    }

    [Flags]
    public enum Methods
    {
        Read = 1,
        Modify = 2,
        ReadWrite = (Read | Modify),
    }
}
