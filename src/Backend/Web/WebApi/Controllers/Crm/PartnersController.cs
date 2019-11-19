using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Crm;
using Company.Common.Models.Pim;
using WebApi.Clients;
using WebApi.Dto.Pim;
using WebApi.Extensions;

namespace WebApi.Controllers.Crm
{
    [Route("v1/[controller]")]
    public class PartnersController : Controller
    {
        private readonly IHttpCrmClient _crmClient;
        private readonly Logger _logger;

        public PartnersController(IHttpCrmClient crmClient)
        {
            _crmClient = crmClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(int userId)
        {
            _logger.Debug($"Start getting team partners by userId={userId} method...");
            HttpResponseMessage response;
            try
            {
                _logger.Trace($"Getting team partners by userId={userId} from MS Crm...");
                response = await _crmClient.Partners.GetTeamPartnerCmpyUserId(userId);
                _logger.Trace($"Getting team partners by userId={userId} from MS Crm successfully received");
            }
            catch (Exception e)
            {
                _logger.Error($"Error on getting team partners by userId={userId}. Message: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace($"Converting response to List of Partner...");
            var responseObject = await response.ResponseToDto<List<Partner>>();
            _logger.Trace($"Response successfully converted to List of Partner");
            _logger.Debug($"Getting team partners by userId={userId}  method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

           
        }
    }
}