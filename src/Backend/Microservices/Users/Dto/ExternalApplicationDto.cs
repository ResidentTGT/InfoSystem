using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Users.Dto
{
    public class ExternalApplicationDto
    {
        public string Id { get; set; }

        public string ApiKey { get; set; }

        public string Name { get; set; }

        public ICollection<PermissionDto> Permissions { get; set; } = new List<PermissionDto>();
    }
}
