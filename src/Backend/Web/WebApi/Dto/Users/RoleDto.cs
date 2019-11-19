
using System;
using System.Collections.Generic;
using System.Text;
using Company.Common.Models.Identity;

namespace WebApi.Dto.Users
{
    public class RoleDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool ICmpelongsToUser { get; set; }

        public Module Module { get; set; }

        public RoleDto(Role role)
        {
            Id = role.Id;
            Name = role.Name;
            ICmpelongsToUser = role.ICmpelongsToUser;
            Module = role.Module;
        }
    }

}
