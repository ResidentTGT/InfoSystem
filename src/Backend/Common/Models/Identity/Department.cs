using System;
using System.Collections.Generic;

namespace Company.Common.Models.Identity
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? DeleteTime { get; set; }

        public int? CreatorId { get; set; }

        public int? DeleterId { get; set; }

        public int? ParentId { get; set; }

        public Department ParentDepartment { get; set; }

        public ICollection<Department> SubDepartments { get; set; } = new List<Department>();

        public ICollection<User> Users { get; set; } = new List<User>();

        public Department()
        {

        }
    }
}
