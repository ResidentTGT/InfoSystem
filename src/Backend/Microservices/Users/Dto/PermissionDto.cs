
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Identity;

namespace Company.Users.Dto
{
    public class PermissionDto
    {
        public string Name { get; set; }

        public Methods Methods { get; set; }
    }
}
