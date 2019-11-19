using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Users.Controllers
{
    [Produces("application/json")]
    [Route("v1/users")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : Controller
    {
        public UsersController()
        {

        }

        [HttpGet("info")]
        public ActionResult GetUserInfo()
        {
            var userDto = HttpContext.Items["User"];

            if (userDto == null)
            {
                return NotFound();
            }

            return new JsonResult(userDto);
        }
    }
}
