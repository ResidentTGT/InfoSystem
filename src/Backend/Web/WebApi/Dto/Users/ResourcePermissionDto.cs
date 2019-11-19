using Company.Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace WebApi.Dto.Users
{
    public class ResourcePermissionDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ResourcePermissionsValues Value { get; set; }

        public int RoleId { get; set; }

      
    }
}