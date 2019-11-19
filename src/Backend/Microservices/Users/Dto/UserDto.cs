using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Users.ViewModels
{
    public class UserDto
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
