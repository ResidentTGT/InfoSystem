using Company.Common.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Dto.Users
{
    public class DepartmentDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<UserDto> Users { get; set; } = new List<UserDto>();

        public DepartmentDto(Department department, bool withUsers = true)
        {
            Id = department.Id;
            Name = department.Name;
            if (withUsers)
                Users = department.Users.Select(u => new UserDto()
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    DisplayName = u.DisplayName,
                    AuthorizationType = u.AuthorizationType,
                    LastName = u.LastName,
                    IsLead = u.IsLead,
                    DepartmentId = u.DepartmentId
                }).ToList();
            else Users = new List<UserDto>();

        }
    }

}
