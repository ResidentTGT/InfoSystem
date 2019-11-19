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
    public class LogisticsController : Controller
    {
        private IHttpSeasonsClient _httpSeasonsClient;
        private readonly Logger _logger;

        public LogisticsController(IHttpSeasonsClient httpSeasonsClient)
        {
            _httpSeasonsClient = httpSeasonsClient;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LogisticDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            _logger.Debug($"Start getting logistic by id={id} method...");
            _logger.Trace($"Getting logistic by id={id} from MS Seasons...");
            var response = await _httpSeasonsClient.Logistics.GetLogisticById(id);
            _logger.Trace($"Logistic by id={id} from MS Seasons successfully received");
            _logger.Trace($"Converting response to LogisticDto...");
            var responseObject = await response.ResponseToDto<Logistic>((l) => new LogisticDto(l));
            _logger.Trace($"Response successfully converted to LogisticDto");
            _logger.Debug($"Getting logistic by id={id} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpGet]
        [ProducesResponseType(typeof(LogisticDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetBySeasonAndBrandIds([FromQuery] int seasonId, [FromQuery] int brandId)
        {
            _logger.Debug($"Start getting logistic by seasonId={seasonId} and brandId={brandId} method...");
            _logger.Trace($"Getting logistic by seasonId={seasonId} and brandId={brandId} from MS Seasons...");
            var response = await _httpSeasonsClient.Logistics.GetBySeasonAndBrandIds(seasonId, brandId);
            _logger.Trace($"Logistic by seasonId={seasonId} and brandId={brandId} from MS Seasons successfully received");
            _logger.Trace($"Converting response to LogisticDto...");
            var responseObject = await response.ResponseToDto<Logistic>((l) => new LogisticDto(l));
            _logger.Trace($"Response successfully converted to LogisticDto");
            _logger.Debug($"Getting logistic by seasonId={seasonId} and brandId={brandId} method is finished");
            return StatusCode((int) response.StatusCode, responseObject);
        }

        [HttpPost]
        [ProducesResponseType(typeof(LogisticDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Create([FromBody] LogisticDto logistic)
        {
            _logger.Debug($"Start creating logistic method...");
            _logger.Trace($"Creating logistic in MS Seasons...");
            var response = await _httpSeasonsClient.Logistics.CreateLogistic(logistic.ToEntity());
            _logger.Trace($"Logistic was successully created in MS Seasons");
            _logger.Trace($"Converting response to LogisticDto...");
            var responseObject = await response.ResponseToDto<Logistic>((l) => new LogisticDto(l));
            _logger.Trace($"Response successfully converted to LogisticDto");
            _logger.Debug($"Creating logistic method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(typeof(LogisticDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] LogisticDto logistic)
        {
            _logger.Debug($"Start editing logistic with id={id} method...");
            _logger.Trace($"Editing logistic with id={id} in MS Seasons...");
            var response = await _httpSeasonsClient.Logistics.EditLogistic(id, logistic.ToEntity());
            _logger.Trace($"Logistic with id={id} was successully edited in MS Seasons");
            _logger.Trace($"Converting response to LogisticDto...");
            var responseObject = await response.ResponseToDto<Logistic>((l) => new LogisticDto(l));
            _logger.Trace($"Response successfully converted to LogisticDto");
            _logger.Debug($"Editing logistic with id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(LogisticDto), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int) HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.Debug($"Start deleting logistic by id={id} method...");
            _logger.Trace($"Deleting logistic by id={id} in MS Seasons...");
            var response = await _httpSeasonsClient.Logistics.DeleteLogistic(id);
            _logger.Trace($"Logistic with id={id} was successully deleted in MS Seasons");
            _logger.Trace($"Converting response to LogisticDto...");
            var responseObject = await response.ResponseToDto<Logistic>((l) => new LogisticDto(l));
            _logger.Trace($"Response successfully converted to LogisticDto");
            _logger.Debug($"Deleting logistic by id={id} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);

        }
    }
}