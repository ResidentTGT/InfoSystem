using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Company.Common.Models.Identity
{
    public class ExternalApplication
    {
        [Key]
        public int Id { get; set; }

        public string ApiKey { get; set; }

        public string Name { get; set; }

        public virtual ICollection<ExternalApplicationRole> ExternalApplicationRoles { get; set; }
    }
}
