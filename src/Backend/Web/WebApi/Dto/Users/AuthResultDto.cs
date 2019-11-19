using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Dto.Users
{
    public class AuthResultDto
    {
        public int UserId { get; set; }

        public string Token { get; set; }
    }
}
