using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Company.Common.Extensions;
using Company.Common.Models.Seasons;
using NLog;

namespace Company.Seasons.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class LogisticsController : Controller
    {
        private readonly SeasonsContext _context;
        private readonly Logger _logger;

        public LogisticsController(SeasonsContext context)
        {
            _context = context;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get logistic by id='{id}' method...");

            _logger.Log(LogLevel.Trace, $"Getting logistic by id='{id}'...");
            var logistic = await _context.Logistics.Where(l => l.DeleteTime == null)
                                                   .Include(l => l.Supplies)
                                                   .FirstOrDefaultAsync(l => l.Id == id);

            if (logistic == null)
            {
                _logger.Log(LogLevel.Error, $"Logistic with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Logistic with id='{id}' successfully fetched");

            _logger.Log(LogLevel.Debug, $"Get logistic with id='{id}' method is over.");
            return Ok(logistic);
        }

        [HttpGet]
        public async Task<IActionResult> GetBySeasonAndBrandIds([FromQuery] int seasonId, [FromQuery] int brandId)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get logistic by season id='{seasonId}' and brand id='{brandId}' ids method...");

            _logger.Log(LogLevel.Trace, $"Getting logistic by season id='{seasonId}' and brand id='{brandId}'...");
            var logistic = await _context.Logistics.Where(l => l.DeleteTime == null)
                                                   .Include(l => l.Supplies)
                                                   .FirstOrDefaultAsync(l => l.SeasonListValueId == seasonId && l.BrandListValueId == brandId);

            if (logistic == null)
            {
                _logger.Log(LogLevel.Error, $"Logistic with season id='{seasonId}' and brand id='{brandId}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Logistic with season id='{seasonId}' and brand id='{brandId}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Updating supplies...");
            logistic.Supplies = logistic.Supplies.Where(s => s.DeleteTime == null).ToList();
            _logger.Log(LogLevel.Trace, "Supplies are updated");

            _logger.Log(LogLevel.Debug, $"Get logistic by season id='{seasonId}' and brand id='{brandId}' method is over");
            return Ok(logistic);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Logistic logistic)
        {
            _logger.Log(LogLevel.Debug, $"Starting the create logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, $"Getting logistic entity with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}'...");
            var logisticEntity = await _context.Logistics.FirstOrDefaultAsync(l => l.SeasonListValueId == logistic.SeasonListValueId
                                                                             && l.BrandListValueId == logistic.BrandListValueId
                                                                             && l.DeleteTime == null);

            if (logisticEntity == null)
            {
                _logger.Log(LogLevel.Trace, $"Logistic entity with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' not found");
                var userId = Convert.ToInt32(Company.Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

                _logger.Log(LogLevel.Trace, "Setting fields of logistic entity");
                logistic.CreateTime = DateTime.Now;
                logistic.CreatorId = userId;
                _logger.Log(LogLevel.Trace, "Fields of logistic entity are set");

                _context.Logistics.Add(logistic);              

                _logger.Log(LogLevel.Trace, $"Saving logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}'. Error: {e.Message}");
                    throw new Exception($"Error on save logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}'");
                }

                _logger.Log(LogLevel.Trace, $"Logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' was successfully saved");

                _logger.Log(LogLevel.Debug, $"Create logistic with 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' method is over.");
                return Ok(logistic);
            }

            _logger.Log(LogLevel.Error, $"Logistics with such 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' already exists");
            return BadRequest($"Логистика с такими 'SeasonListValueId': '{logistic.SeasonListValueId}' и 'BrandListValueId': '{logistic.BrandListValueId}' уже существует.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Logistic logistic)
        {
            _logger.Log(LogLevel.Debug, $"Starting the edit logistic with id='{id}' method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, $"Getting logistic entity by id='{id}'...");
            var logisticEntity = await _context.Logistics.FindAsync(id);

            if (logisticEntity == null)
            {
                _logger.Log(LogLevel.Error, $"Logistic entity with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, "Logistic entity successfully fetched");

            _logger.Log(LogLevel.Trace, "Checking logistics...");
            if (!_context.Logistics.Any(l => l.SeasonListValueId == logistic.SeasonListValueId
                                             && l.BrandListValueId == logistic.BrandListValueId
                                             && l.Id != logistic.Id
                                             && l.DeleteTime == null))
            {
                _logger.Log(LogLevel.Trace, "Logistics are checked");
                var userId = Convert.ToInt32(Company.Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

                _logger.Log(LogLevel.Trace, $"Updating logistic entity with id='{id}'...");
                logisticEntity.SeasonListValueId = logistic.SeasonListValueId;
                logisticEntity.BrandListValueId = logistic.BrandListValueId;
                logisticEntity.ProductsVolume = logistic.ProductsVolume;
                logisticEntity.MoneyVolume = logistic.MoneyVolume;
                logisticEntity.BatchesCount = logistic.BatchesCount;
                logisticEntity.AdditionalFactor = logistic.AdditionalFactor;
                logisticEntity.Insurance = logistic.Insurance;
                logisticEntity.OtherAdditional = logistic.OtherAdditional;

                _logger.Log(LogLevel.Trace, $"Updating supplies fields of logistic entity with id='{id}'...");
                foreach (var supply in logisticEntity.Supplies)
                {
                    if (!logistic.Supplies.Select(s => s.Id).Contains(supply.Id))
                    {
                        supply.DeleterId = userId;
                        supply.DeleteTime = DateTime.Now;
                    }
                }
                _logger.Log(LogLevel.Trace, $"Supplies fields of logistic entity with id='{id}' are updated");

                _logger.Log(LogLevel.Trace, $"Editing supplies of logistic with id='{id}'...");
                try
                {
                    foreach (var supply in logistic.Supplies)
                    {
                        var supplyEntity = _context.Supplies.FirstOrDefault(s => s.Id == supply.Id);

                        if (supplyEntity == null)
                        {
                            await _context.Supplies.AddAsync(new Supply()
                            {
                                BatchesCount = supply.BatchesCount,
                                RiskCoefficient = supply.RiskCoefficient,
                                DeliveryDate = supply.DeliveryDate,
                                FabricDate = supply.FabricDate,
                                TransportCost = supply.TransportCost,
                                BrokerCost = supply.BrokerCost,
                                WtsCost = supply.WtsCost,
                                Other = supply.Other,
                                LogisticId = logisticEntity.Id
                            });
                        }
                        else
                        {
                            supplyEntity.BatchesCount = supply.BatchesCount;
                            supplyEntity.RiskCoefficient = supply.RiskCoefficient;
                            supplyEntity.DeliveryDate = supply.DeliveryDate;
                            supplyEntity.FabricDate = supply.FabricDate;
                            supplyEntity.TransportCost = supply.TransportCost;
                            supplyEntity.BrokerCost = supply.BrokerCost;
                            supplyEntity.WtsCost = supply.WtsCost;
                            supplyEntity.Other = supply.Other;
                        }
                    }

                    _logger.Log(LogLevel.Trace, $"Editing supplies of logistic with id='{id}' is finished");
                    _logger.Log(LogLevel.Trace, $"Updating supplies fields of logistic entity with id='{id}' is finished");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"On editing supplies of logistic with id='{id}' raised the exception. Error: {e.Message}");
                    return BadRequest($"On editing supplies of logistic with id='{id}' raised the exception.");
                }

                _logger.Log(LogLevel.Trace, $"Saving logistic with id='{id}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving logistic with id='{id}'. Error: {e.Message}");
                    throw new Exception($"Error on save logistic with id='{id}'");
                }

                _logger.Log(LogLevel.Trace, $"Logistic with id='{id}' were successfully saved");

                _logger.Log(LogLevel.Debug, $"Edit logistic with id='{id}' method is over.");
                return Ok(logisticEntity);
            }

            _logger.Log(LogLevel.Error, $"Logistics with such 'SeasonListValueId': '{logistic.SeasonListValueId}' and 'BrandListValueId': '{logistic.BrandListValueId}' already exists");
            return BadRequest($"Логистика с такими 'SeasonListValueId': '{logistic.SeasonListValueId}' и 'BrandListValueId': '{logistic.BrandListValueId}' уже существует.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Starting the delete logistic with id='{id}' method...");

            var userId = Convert.ToInt32(Company.Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, $"Getting logistic by id='{id}'...");
            var logistic = await _context.Logistics.Include(a => a.Supplies).FirstOrDefaultAsync(a => a.Id == id);

            if (logistic == null)
            {
                _logger.Log(LogLevel.Info, $"Logistic with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Logistic with id='{id}' successfully fetched");

            _logger.Log(LogLevel.Trace, $"Updating fields of logistic with id='{id}'...");
            logistic.DeleterId = userId;
            logistic.DeleteTime = DateTime.Now;

            _logger.Log(LogLevel.Trace, $"Editing supplies of logistic with id='{id}'...");
            logistic.Supplies.ForEach(s => { s.DeleteTime = DateTime.Now; s.DeleterId = userId; });
            _logger.Log(LogLevel.Trace, $"Editing supplies of logistic with id='{id}' is finished");
            _logger.Log(LogLevel.Trace, $"Updating fields of logistic with id='{id}' is finished");

            _logger.Log(LogLevel.Trace, $"Saving logistic with id='{id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving logistic with id='{id}'. Error: {e.Message}");
                throw new Exception($"Error on save logistic with id='{id}'");
            }

            _logger.Log(LogLevel.Trace, $"Logistic with id='{id}' were successfully saved");

            _logger.Log(LogLevel.Debug, $"Delete logistic with id='{id}' method is over.");
            return Ok(logistic);
        }
    }
}
