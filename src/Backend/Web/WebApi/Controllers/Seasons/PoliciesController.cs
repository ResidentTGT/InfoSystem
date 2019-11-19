using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityDatabase.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Company.Common.Models.Seasons;
using WebApi.Clients;
using WebApi.Dto.Seasons;
using WebApi.Extensions;

namespace WebApi.Controllers.Seasons
{
    [Route("v1/[controller]")]
    public class PoliciesController : Controller
    {
        private IHttpSeasonsClient _httpSeasonsClient;
        private readonly Logger _logger;

        public PoliciesController(IHttpSeasonsClient httpSeasonsClient)
        {
            _httpSeasonsClient = httpSeasonsClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        [ProducesResponseType(typeof(DiscountPolicyDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromQuery] int seasonId)
        {
            _logger.Debug($"Start getting policy by seasonId={seasonId} method...");
            HttpResponseMessage response;
            try
            {
                _logger.Trace($"Getting policy by seasonId={seasonId} from MS Seasons...");
                response = await _httpSeasonsClient.Policies.GetPolicyBySeasonId(seasonId);
                _logger.Trace($"Policy by seasonId={seasonId} from MS Seasons successfully received");
            }
            catch (Exception e)
            {
                _logger.Error($"Error on getting policy by seasonId={seasonId}. Message: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace($"Converting response to DiscountPolicyDto...");
            var responseObject = await response.ResponseToDto<DiscountPolicy>((d) => new DiscountPolicyDto(d));
            _logger.Trace($"Response successfully converted to DiscountPolicyDto");
            _logger.Debug($"Getting policy by seasonId={seasonId} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountPolicyDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] DiscountPolicyDto discountPolicy)
        {
            _logger.Debug($"Start creating policy method...");
            HttpResponseMessage response;
            try
            {
                _logger.Trace($"Creating policy in MS Seasons...");
                response = await _httpSeasonsClient.Policies.CreatePolicy(discountPolicy.ToEntity()); 
                _logger.Trace($"Policy was successfully created in MS Seasons");
            }
            catch (Exception e)
            {
                _logger.Error($"Error on creating policy. Message: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace($"Converting response to DiscountPolicyDto...");
            var responseObject = await response.ResponseToDto<DiscountPolicy>((d) => new DiscountPolicyDto(d));
            _logger.Trace($"Response successfully converted to DiscountPolicyDto");
            _logger.Debug($"Creating policy method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DiscountPolicyDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] DiscountPolicyDto discountPolicy)
        {
            _logger.Debug($"Start editing policy by with={id} method...");
            HttpResponseMessage response;
            try
            {
                _logger.Trace($"Editing policy with id={id} in MS Seasons...");
                response = await _httpSeasonsClient.Policies.EditPolicy(id, discountPolicy.ToEntity());
                _logger.Trace($"Policy with id={id} was successfully edited in MS Seasons");
            }
            catch (Exception e)
            {
                _logger.Error($"Error on editing policy with id={id}. Message: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace($"Converting response to DiscountPolicyDto...");
            var responseObject = await response.ResponseToDto<DiscountPolicy>((d) => new DiscountPolicyDto(d));
            _logger.Trace($"Response successfully converted to DiscountPolicyDto");
            _logger.Debug($"Editing policy with id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpGet("exchange-rates")]
        [ProducesResponseType(typeof(ExchangeRatesDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetExchangeRates([FromQuery(Name = "seasonId")] int seasonListValueId)
        {
            _logger.Debug($"Start getting exchange rates by seasonListValueId={seasonListValueId} method...");
            HttpResponseMessage response;
            try
            {
                _logger.Trace($"Getting exchange rates by seasonListValueId={seasonListValueId} from MS Seasons...");
                response = await _httpSeasonsClient.Policies.GetExchangeRateCmpySeasonListValueId(seasonListValueId);
                _logger.Trace($"Exchange rates by seasonListValueId={seasonListValueId} from MS Seasons successfully received");
            }
            catch (Exception e)
            {
                _logger.Error($"Error on getting exchange rates by seasonListValueId={seasonListValueId}. Message: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace($"Converting response to ExchangeRatesDto...");
            var responseObject = await response.ResponseToDto<ExchangeRates>((d) => new ExchangeRatesDto(d));
            _logger.Trace($"Response successfully converted to ExchangeRatesDto");
            _logger.Debug($"Getting exchange rates by seasonListValueId={seasonListValueId} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }
    }
}