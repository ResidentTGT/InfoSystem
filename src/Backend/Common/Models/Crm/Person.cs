using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Models.Crm
{
    public class Person
    {
        public Person()
        {
            TeamRolePeoples = new List<TeamRolePeople>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }

        public virtual ICollection<TeamRolePeople> TeamRolePeoples { get; set; }
    }
}