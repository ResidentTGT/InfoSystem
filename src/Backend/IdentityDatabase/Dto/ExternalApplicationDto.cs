using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityDatabase.Dto
{
    public class ExternalApplicationDto
    {
        public int Id { get; set; }

        public string ApiKey { get; set; }

        public string Name { get; set; }

        public List<ModulePermissoinDto> ModulePermissions { get; set; } = new List<ModulePermissoinDto>();
    }
}
