using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace Company.Crm.ApiControllers
{
    [Route("v1/[controller]")]
    public class PartnersController : Controller
    {
        private readonly CrmContext _crmContext;
        private readonly Logger _logger;

        public PartnersController(CrmContext crmContext)
        {
            _crmContext = crmContext;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetPartnerCmpyUserId([FromRoute]int userId)
        {
            _logger.Debug($"Starting the getting partners by userId={userId} method...");
            _logger.Trace($"Getting partners by userId={userId} ...");
            var partners = await _crmContext.TeamRolePeoples.Where(trp => trp.Person.UserId == userId).Select(trp => trp.Partner).ToListAsync();
            _logger.Trace("Partners successfully received");
            _logger.Debug($"Get partners by userId={userId} method is finished...");
            return Ok(partners);
        }
    }
}