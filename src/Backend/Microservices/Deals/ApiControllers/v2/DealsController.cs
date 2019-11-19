using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
using Company.Common.Enums;
using Company.Common.Extensions;
using Company.Common.Models.Deals;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Deals.Client.v2;
using Company.Deals.Dto.v2;
using Company.Deals.Interfaces.v2;
using Company.Deals.Structures;
using NLog;

namespace Company.Deals.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class DealsController : Controller
    {
        private readonly IPimMSCommunicator _httpPimCommunicator;
        private const int SkuCellRowIndex = 9;
        private const string TinCellAddress = "L2";
        private const string RrcCellAddress = "L3";
        private const string BrandCellAddress = "B2";
        private const string DiscountCellAddress = "L6";
        private const int CountColumnNum = 2;
        private const int SkuColumnNum = 1;

        private DealsContext _context { get; set; }
        private readonly Logger _logger;
        private IConfiguration _configuration { get; set; }
        private HttpWebApiCommunicator _httpWebApiCommunicator { get; set; }

        public DealsController(DealsContext context, IPimMSCommunicator httpPimCommunicator, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _httpPimCommunicator = httpPimCommunicator;
            _httpWebApiCommunicator = new HttpWebApiCommunicator();
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDeals([FromQuery] int? pageSize, [FromQuery] int? pageNumber,
            [FromQuery] string contractor, [FromQuery] float? discountFrom, [FromQuery] float? discountTo,
            [FromQuery] int? dealId, [FromQuery] DateTime? loadDateFrom, [FromQuery]  DateTime? loadDateTo,
            [FromQuery] DateTime? createDateFrom, [FromQuery]  DateTime? createDateTo)
        {
            _logger.Log(LogLevel.Debug, "Starting the get all deals method...");
            _logger.Trace("Parsing ids from query params...");
            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data ...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user with id='{userId}'. Error: {e.Message}");
            }

            _logger.Trace("Ids was successfully parsed");

            var brandsIds = Request.GetIdsFromQuery("brands");
            var seasonsIds = Request.GetIdsFromQuery("seasons");
            var departmentsIds = Request.GetIdsFromQuery("departments");
            var managersIds = Request.GetIdsFromQuery("managers");

            var queryDeals = _context.Deals.Include(d => d.DiscountParams)
                                           .Include(d => d.HeadDiscountRequests)
                                           .Where(d => d.DeleteTime == null);

            if (user.IsLead)
            {
                var resp = await _httpWebApiCommunicator.GetUsersFromDepartmentTree(Convert.ToInt32(user.DepartmentId));
                var users = JsonConvert.DeserializeObject<List<User>>(await resp.Content.ReadAsStringAsync());

                var usersIds = (departmentsIds.Any() ? users.Where(u => departmentsIds.Contains(u.DepartmentId.Value)) : users).Select(u => u.Id).ToList();
                managersIds = managersIds.Any() ? managersIds.Intersect(usersIds).ToList() : usersIds;

                queryDeals = queryDeals.Where(d => managersIds.Contains(d.ManagerId));
            }
            else
            {
                queryDeals = queryDeals.Where(d => d.ManagerId == user.Id);
            }

            queryDeals = queryDeals.AddQuery(d => seasonsIds.Contains(d.SeasonId), seasonsIds.Any());
            queryDeals = queryDeals.AddQuery(d => brandsIds.Contains(d.BrandId), brandsIds.Any());

            queryDeals = queryDeals.AddQuery(d => d.Contractor.ToUpper().Trim().Contains(contractor.ToUpper().Trim()), !string.IsNullOrWhiteSpace(contractor));

            queryDeals = queryDeals.AddQuery(d => d.Discount >= discountFrom, discountFrom.HasValue);
            queryDeals = queryDeals.AddQuery(d => d.Discount <= discountTo, discountTo.HasValue);

            queryDeals = queryDeals.AddQuery(d => d.Id == dealId, dealId.HasValue);

            queryDeals = queryDeals.AddQuery(d => d.Upload1cTime >= loadDateFrom, loadDateFrom.HasValue);
            queryDeals = queryDeals.AddQuery(d => d.Upload1cTime <= loadDateTo, loadDateTo.HasValue);

            queryDeals = queryDeals.AddQuery(d => d.CreateDate >= createDateFrom, createDateFrom.HasValue);
            queryDeals = queryDeals.AddQuery(d => d.CreateDate <= createDateTo, createDateTo.HasValue);


            queryDeals = queryDeals.OrderByDescending(d => d.CreateDate);


            if (pageSize.HasValue)
            {
                var skip = (pageNumber ?? 0) * pageSize;
                queryDeals = queryDeals.Skip(skip.Value).Take(pageSize.Value);
            }
            _logger.Log(LogLevel.Trace, "Deals successfully fetched");

            _logger.Trace("Getting deals by specified parameters...");
            var deals = await queryDeals.ToListAsync();

            _logger.Log(LogLevel.Debug, "Get all deals method is over.");
            return Ok(deals);

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDealById([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get deal by id='{id}' method...");

            _logger.Log(LogLevel.Trace, "Getting deal...");
            var deal = await _context.Deals.Where(a => a.DeleteTime == null)
                                           .Include(d => d.DiscountParams)
                                           .Include(d => d.DealProducts)
                                           .Include(d => d.HeadDiscountRequests)
                                           .FirstOrDefaultAsync(c => c.Id == id);

            if (deal == null)
            {
                _logger.Log(LogLevel.Info, $"Deal with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Deal with id='{deal.Id}' successfully fetched");

            Request.HttpContext.Response.Headers.Add("X-DealProducts-Count", deal.DealProducts.Select(dp => dp.Count).Sum().ToString());
            Request.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "X-DealProducts-Count");

            _logger.Log(LogLevel.Debug, "Get deal by id method is over.");
            return Ok(deal);
        }

        [HttpGet("files/{id}")]
        public async Task<ActionResult> GetDownloadFile([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get download file with id='{id}' method...");

            var httpFileStorageClient = new HttpFileStorageMsCommunicator();

            _logger.Log(LogLevel.Trace, "Downloading file...");
            var file = await httpFileStorageClient.DownloadFile(id);
            _logger.Log(LogLevel.Trace, $"File with id='{id}' is downloaded");

            FileStreamResult res;

            _logger.Log(LogLevel.Trace, "Updating discount in the file...");
            using (ExcelPackage package = new ExcelPackage(file.FileStream))
            {
                var deal = _context.Deals.FirstOrDefault(d => d.OrderFormId == id);

                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    worksheet.Cells[DiscountCellAddress].Value = deal.Discount;
                }

                package.Save();
                package.Stream.Seek(0, SeekOrigin.Begin);

                res = new FileStreamResult(new MemoryStream(package.GetACmpyteArray()), "application/octet-stream");
                res.FileDownloadName = file.FileDownloadName;
            }
            _logger.Log(LogLevel.Trace, "Updating discount in the file is finished");

            _logger.Log(LogLevel.Debug, "Get download file method is over.");
            return res;
        }

        [HttpGet("marginalities")]
        public async Task<IActionResult> GetDealsManagerMarginalities([FromQuery] int dealId)
        {
            _logger.Log(LogLevel.Debug, "Starting the get deals manager marginalities method...");

            _logger.Log(LogLevel.Trace, $"Getting deal by id='{dealId}'...");
            var deal = await _context.Deals.FindAsync(dealId);

            if (deal == null)
            {
                _logger.Log(LogLevel.Info, $"Deal with id='{dealId}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Deal with id='{dealId}' successfully fetched");

            _logger.Log(LogLevel.Debug, "Get deals manager marginalities method is over.");
            return Ok(_context.Deals.Where(d => d.ManagerId == deal.ManagerId && d.SeasonId == deal.SeasonId && d.DealStatus != DealStatus.NotConfirmed && d.DeleteTime == null)
                                    .Select(d => d.ManagerMarginality)
                                    .ToList());
        }

        [HttpPost]
        public async Task<IActionResult> LoadOrderForm()
        {
            _logger.Log(LogLevel.Debug, "Starting the load order form method...");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Getting file from request...");
            var reqFile = HttpContext.Request.Form.Files.FirstOrDefault();

            if (reqFile != null)
            {
                _logger.Log(LogLevel.Trace, $"File '{reqFile.FileName}' successfully fetched");
                OrderFormParseResult parseResult;
                _logger.Log(LogLevel.Trace, "Parsing order form from file...");
                try
                {
                    parseResult = await ParseOrderForm(reqFile);
                }
                catch (Exception ex)
                {
                    _logger.Log(LogLevel.Error, $"Couldn't parse order form from file '{reqFile.FileName}'. Error: {ex.Message}");
                    return BadRequest(ex.Message);
                }
                _logger.Log(LogLevel.Trace, $"Parsing order form from file '{reqFile.FileName}' is finished");

                var httpFileStorageClient = new HttpFileStorageMsCommunicator();
                _logger.Log(LogLevel.Trace, "Saving deal file...");
                var orderFormId = httpFileStorageClient.SaveDealFile(HttpContext).Id;
                _logger.Log(LogLevel.Trace, "Deal file was successfully saved");

                var oneCCommunicator = new Http1cTransportCommunicator();

                _logger.Log(LogLevel.Trace, "Getting list of SKUs...");
                var listOfSkusFromParsedOrderForm = parseResult.skusAndCountsDictionary.Keys.ToArray();
                _logger.Log(LogLevel.Trace, $"List of SKUs '{String.Join(',', listOfSkusFromParsedOrderForm)}' successfully fetched");

                _logger.Log(LogLevel.Trace, "Getting products...");
                var products = await _httpPimCommunicator.GetProductCmpySkus(listOfSkusFromParsedOrderForm, HttpContext);

                if (products.Count == 0)
                {
                    _logger.Log(LogLevel.Info, $"Products with SKUs '{String.Join(',', listOfSkusFromParsedOrderForm)}' not found in the system");
                    return BadRequest("В системе не найдено ни одного продукта из данной ЗФ.");
                }
                _logger.Log(LogLevel.Trace, $"Products with SKUs '{String.Join(',', listOfSkusFromParsedOrderForm)}' successfully fetched");

                var netCostId = int.Parse(_configuration.GetSection("AttributesIds:NetCost").Value);
                var netCostPureId = int.Parse(_configuration.GetSection("AttributesIds:NetCostPure").Value);
                var bwpId = int.Parse(_configuration.GetSection("AttributesIds:Bwp").Value);
                var bwpPureId = int.Parse(_configuration.GetSection("AttributesIds:BwpPure").Value);
                var seasonId = int.Parse(_configuration.GetSection("AttributesIds:Season").Value);

                _logger.Log(LogLevel.Trace, "Checking season id in products...");
                if (products.First().AttributeValues.All(f => f.Attribute.Id != seasonId))
                {
                    _logger.Log(LogLevel.Info, $"There is an unfilled season with id='{seasonId}' in the product");
                    return BadRequest($"There is an unfilled season with id='{seasonId}' in the products.");
                }
                _logger.Log(LogLevel.Trace, $"Season id='{seasonId}' in products is checked");

                var contractor = new ContractorDto();

                _logger.Log(LogLevel.Trace, "Getting season id...");
                var currentSeasonId = products.FirstOrDefault().AttributeValues.FirstOrDefault(p => p.Attribute.Id == seasonId).ListValueId.Value;
                _logger.Log(LogLevel.Trace, $"Season id='{currentSeasonId}' successfully fetched");

                try
                {
                    _logger.Log(LogLevel.Trace, $"Getting season list value with season id='{currentSeasonId}'...");
                    var seasonListValue = await _httpPimCommunicator.GetListValue(currentSeasonId, HttpContext);

                    if (seasonListValue == null)
                    {
                        _logger.Log(LogLevel.Info, $"Season list value with season id='{currentSeasonId}' not found");
                        return NotFound("Season not found.");
                    }
                    _logger.Log(LogLevel.Trace, $"Season list value with season id='{currentSeasonId}' successfully fetched");

                    _logger.Log(LogLevel.Trace, "Getting contractor...");
                    contractor = oneCCommunicator.GetPartner(parseResult.Tin, parseResult.Rrc, seasonListValue.Value);

                    if (contractor.Status == 409)
                    {
                        _logger.Log(LogLevel.Info, $"Found several counterparties with this TIN='{parseResult.Tin}'. Refine your request");
                        return BadRequest($"Найдено несколько контрагентов с данным ИНН='{parseResult.Tin}'. Уточните запрос");
                    }
                    else if (contractor.Status == 404)
                    {
                        _logger.Log(LogLevel.Info, $"According to the requisites, the counterparty was not found. Make a new counterpart in 1C");
                        return BadRequest("По данным реквизитам контрагент не найден. Внесите нового контрагента в 1С.");
                    }
                    else if (contractor.Status == 200 && (contractor.Contracts?.Count() ?? 0) == 0)
                    {
                        _logger.Log(LogLevel.Info, $"This counterparty does not have concluded contracts for this season");
                        return BadRequest("У данного контрагена нет заключенных договоров на данный сезон.");
                    }

                    _logger.Log(LogLevel.Trace, $"Contractor '{contractor.Name}' successfully fetched");
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Counterparty not found according to TIN='{parseResult.Tin}' and KPP='{parseResult.Rrc}' data. Error: {e.Message}");
                    return BadRequest($"Не найден контрагент по данным ИНН='{parseResult.Tin}' и КПП='{parseResult.Rrc}'.");
                }

                _logger.Log(LogLevel.Trace, "Creating deal...");
                var deal = new Deal()
                {
                    CreateDate = DateTime.Now,
                    ManagerId = userId,
                    OrderFormId = orderFormId,
                    Discount = 0,
                    DealStatus = DealStatus.NotConfirmed,

                    SeasonId = products.FirstOrDefault().AttributeValues.FirstOrDefault(p => p.Attribute.Id == seasonId).ListValueId.Value,

                    // From УТ
                    Contractor = contractor.Name,
                    Repeatedness = contractor.Repeatedness,
                    PartnerNameOnMarket = contractor.Name,
                    BrandMix = contractor.BrandMix,
                    PartnersType = (PartnersType)Enum.ToObject(typeof(PartnersType), contractor.PartnerType),

                    // From Form
                    Tin = parseResult.Tin,
                    Rrc = parseResult.Rrc,
                    Brand = parseResult.Brand,
                    BrandId = parseResult.BrandId
                };
                _logger.Log(LogLevel.Trace, "Deal is created");

                _logger.Log(LogLevel.Trace, "Creating deal product...");
                deal.DealProducts.AddRange(products.Select(p => new DealProduct()
                {
                    ProductId = p.Id,
                    Count = parseResult.skusAndCountsDictionary[p.Sku]
                }));
                _logger.Log(LogLevel.Trace, "Deal product is created");

                //From products
                _logger.Log(LogLevel.Trace, "Filling deal fields...");
                deal.Volume = CountSumByAttributeId(products, deal, bwpId); //TODO: проверка БОц
                deal.VolumePure = CountSumByAttributeId(products, deal, bwpPureId);
                deal.NetCost = CountSumByAttributeId(products, deal, netCostId);
                deal.NetCostPure = CountSumByAttributeId(products, deal, netCostPureId);
                _logger.Log(LogLevel.Trace, "Fields of deal are filled");

                _logger.Log(LogLevel.Trace, "Creating discount param...");
                var discParams = new DiscountParams()
                {
                    CreatorId = userId,
                    CreateDate = DateTime.Now,
                    ConsiderMarginality = true,
                };
                _logger.Log(LogLevel.Trace, "Discount param is created");

                _logger.Log(LogLevel.Trace, "Filling implementation contract and commission contract...");
                if (contractor.Contracts.Any(c => c.Type == "С покупателем / заказчиком"))
                    discParams.ImplementationContract = Guid.Parse(contractor.Contracts.OrderByDescending(c => c.StartDate).FirstOrDefault(c => c.Type == "С покупателем / заказчиком").Guid);

                if (contractor.Contracts.Any(c => c.Type == "С комиссионером"))
                    discParams.CommissionContract = Guid.Parse(contractor.Contracts.OrderByDescending(c => c.StartDate).First(c => c.Type == "С комиссионером").Guid);

                _logger.Log(LogLevel.Trace, "Implementation contract and commission contract are filled");

                deal.DiscountParams.Add(discParams);

                _context.Deals.Add(deal);

                _logger.Log(LogLevel.Trace, $"Saving deal with id='{deal.Id}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving deal with id='{deal.Id}'. Error: {e.Message}");
                    return BadRequest($"Error on saving deal with id='{deal.Id}'");
                }
                _logger.Log(LogLevel.Trace, $"Deal with id='{deal.Id}' were successfully saved");

                _logger.Log(LogLevel.Debug, "Load order form method is over.");
                return Ok(deal);
            }
            else
            {
                _logger.Log(LogLevel.Info, $"File is empty");
                return BadRequest();
            }
        }

        [HttpPost("{dealId}/contract")]
        public async Task<IActionResult> SaveDealDocuments([FromRoute] int dealId)
        {
            _logger.Log(LogLevel.Debug, $"Starting the save deal documents with id='{dealId}' method...");

            _logger.Log(LogLevel.Trace, "Getting file from request...");
            var reqFile = HttpContext.Request.Form.Files.FirstOrDefault();

            if (reqFile != null)
            {
                _logger.Log(LogLevel.Trace, "File successfully fetched");

                _logger.Log(LogLevel.Trace, "Getting deal...");
                var deal = await _context.Deals.FindAsync(dealId);

                if (deal == null)
                {
                    _logger.Log(LogLevel.Info, $"Deal with id='{dealId}' not found");
                    return NotFound("Сделка не найдена");
                }
                _logger.Log(LogLevel.Trace, $"Deal with id='{dealId}' successfully fetched");

                var httpFileStorageClient = new HttpFileStorageMsCommunicator();

                deal.DealStatus = DealStatus.Confirmed;

                _logger.Log(LogLevel.Trace, "Creating deal file...");
                deal.DealFiles.Add(new DealFile()
                {
                    FileId = httpFileStorageClient.SaveDealFile(HttpContext).Id,
                    DealId = deal.Id
                });
                _logger.Log(LogLevel.Trace, "Deal file is created");

                _logger.Log(LogLevel.Trace, $"Saving deal with id='{deal.Id}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving deal with id='{deal.Id}'. Error: {e.Message}");
                    return BadRequest($"Error on save deal with id='{deal.Id}'");
                }
                _logger.Log(LogLevel.Trace, $"Deal with id='{deal.Id}' was successfully saved");

                _logger.Log(LogLevel.Debug, "Save deal documents method is over.");
                return Ok();
            }
            else
            {
                _logger.Log(LogLevel.Info, "File from request is empty");
                return BadRequest("Файл пуст");
            }
        }

        [HttpPost("head-discount-request")]
        public async Task<IActionResult> CreateHeadDiscountRequest([FromBody] HeadDiscountRequest headDiscountRequest)
        {
            _logger.Log(LogLevel.Debug, "Starting the create head discount request method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Creating discount request entity...");
            var headDiscountRequestEntity = new HeadDiscountRequest()
            {
                CreateTime = DateTime.Now,
                CreatorId = userId,
                Receiver = headDiscountRequest.Receiver,
                DealId = headDiscountRequest.DealId,
                Discount = headDiscountRequest.Discount
            };
            _logger.Log(LogLevel.Trace, "Discount request entity is created");

            _context.HeadDiscountRequests.Add(headDiscountRequestEntity);

            _logger.Log(LogLevel.Trace, $"Saving discount request entity with id='{headDiscountRequestEntity.Id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving discount request entity with id='{headDiscountRequestEntity.Id}'. Error: {e.Message}");
                return BadRequest($"Error on saving discount request entity with id='{headDiscountRequestEntity.Id}'.");
            }
            _logger.Log(LogLevel.Trace, $"Discount request entity with id='{headDiscountRequestEntity.Id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Create head discount request method is over.");
            return Ok(headDiscountRequestEntity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Deal deal)
        {
            _logger.Log(LogLevel.Debug, "Starting the edit deal method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Getting deal entity...");
            var dealEntity = await _context.Deals.Include(d => d.DiscountParams)
                                                 .Include(d => d.DealProducts)
                                                 .Include(d => d.HeadDiscountRequests)
                                                 .FirstOrDefaultAsync(c => c.Id == id);

            if (dealEntity == null)
            {
                _logger.Log(LogLevel.Info, $"Deal entity with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Deal entity with id='{id}' successfully fetched");

            _logger.Log(LogLevel.Trace, "Filling deal entity fields...");
            dealEntity.DealStatus = deal.DealStatus;
            dealEntity.Discount = deal.Discount;
            dealEntity.ManagerMarginality = deal.ManagerMarginality;
            dealEntity.DealMarginality = (deal.NetCostPure / (deal.VolumePure * (1 - deal.Discount / 100))) * 100;
            dealEntity.Comment = deal.Comment;
            dealEntity.Delivery = deal.Delivery;
            dealEntity.ProductType = deal.ProductType;
            _logger.Log(LogLevel.Trace, "Fields of deal entity are filled");

            if (!deal.DiscountParams.Equals(dealEntity.DiscountParams.Last()))
            {
                var discParams = deal.DiscountParams.LastOrDefault();

                _logger.Log(LogLevel.Trace, "Creating discount params...");
                dealEntity.DiscountParams.Add(new DiscountParams()
                {
                    CeoDiscount = discParams.CeoDiscount,
                    HeadDiscount = discParams.HeadDiscount,
                    CommissionContract = discParams.CommissionContract,
                    ImplementationContract = discParams.ImplementationContract,
                    ConsiderMarginality = discParams.ConsiderMarginality,
                    ContractType = discParams.ContractType,
                    CreateDate = DateTime.Now,
                    CreatorId = userId,
                    Installment = discParams.Installment,
                    Prepayment = discParams.Prepayment,
                    OrderType = discParams.OrderType
                });
            }
            _logger.Log(LogLevel.Trace, "Discount params are created");

            if (dealEntity.DealStatus == DealStatus.OnPayment)
            {
                var t = new Http1cTransportCommunicator();

                _logger.Log(LogLevel.Trace, "Sending deal archive...");

                dealEntity.Upload1cTime = DateTime.Now;

                if (await t.SendDealArchive(dealEntity, HttpContext, _httpPimCommunicator, _configuration) != 200)
                {
                    dealEntity.DealStatus = DealStatus.NotConfirmed;
                    dealEntity.Upload1cTime = null;

                    _logger.Log(LogLevel.Trace, $"Saving deal entity with id='{dealEntity.Id}' to database...");
                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        _logger.Log(LogLevel.Error, $"Error on saving deal entity with id='{dealEntity.Id}'. Error: {e.Message}");
                        return BadRequest($"Error on save deal with id='{dealEntity.Id}'");
                    }
                    _logger.Log(LogLevel.Trace, $"Deal entity with id='{dealEntity.Id}' was successfully saved");

                    _logger.Log(LogLevel.Trace, "Can't parse deal");
                    return BadRequest("Can't parse deal.");
                }
            }
            _logger.Log(LogLevel.Trace, "Deal archive is sended");

            _logger.Log(LogLevel.Trace, $"Saving deal entity with id='{dealEntity.Id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving deal entity with id='{dealEntity.Id}'. Error: {e.Message}");
                return BadRequest($"Error on save deal entity with id='{dealEntity.Id}'");
            }
            _logger.Log(LogLevel.Trace, $"Deal entity with id='{dealEntity.Id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Edit deal method is over.");
            return Ok(dealEntity);
        }

        [HttpPut("head-discount-request/{id}")]
        public async Task<IActionResult> EditHeadDiscountRequest([FromRoute] int id, [FromBody] HeadDiscountRequest headDiscountRequest)
        {
            _logger.Log(LogLevel.Debug, $"Starting the edit deal with id='{id}' method...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, "Getting head discount requests entity...");
            var headDiscountRequestEntity = await _context.HeadDiscountRequests.FindAsync(id);

            if (headDiscountRequestEntity == null)
            {
                _logger.Log(LogLevel.Info, $"Head discount requests entity with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, "Head discount requests entity successfully fetched");

            _logger.Log(LogLevel.Trace, "Updating head discount requests entity...");
            headDiscountRequestEntity.Receiver = headDiscountRequest.Receiver;
            headDiscountRequestEntity.Discount = headDiscountRequest.Discount;
            headDiscountRequestEntity.Confirmed = headDiscountRequest.Confirmed;
            _logger.Log(LogLevel.Trace, "Head discount requests entity are updated.");

            _logger.Log(LogLevel.Trace, $"Saving head discount requests entity with id='{id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving head discount requests entity with id='{id}'. Error: {e.Message}");
                return BadRequest($"Error on save head discount requests entity with id='{id}'");
            }
            _logger.Log(LogLevel.Trace, $"Head discount requests entity with id='{id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Edit deal method is over.");
            return Ok(headDiscountRequestEntity);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteDeals()
        {
            _logger.Log(LogLevel.Debug, "Starting the delete deals method...");

            _logger.Log(LogLevel.Trace, "Getting IDs of deals from the request query...");
            var ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToArray();
            _logger.Log(LogLevel.Trace, $"IDs of deals '{String.Join(',', ids)}' successfully fetched");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Getting deals...");
            var removedDeals = _context.Deals.Where(p => ids.Contains(p.Id)).Include(d => d.DiscountParams).ToList();
            _logger.Log(LogLevel.Trace, "Deals successfully fetched");

            _logger.Log(LogLevel.Trace, "Setting deals fields...");
            foreach (var deal in removedDeals)
            {
                deal.DeleteTime = DateTime.Now;
                deal.DeleterId = userId;
            }
            _logger.Log(LogLevel.Trace, "Deals fields are set");

            _logger.Log(LogLevel.Trace, $"Saving deals '{String.Join(',', ids)}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving deals. Error: {e.Message}");
                return BadRequest("Error on save deals");
            }
            _logger.Log(LogLevel.Trace, "Deals were successfully saved");

            _logger.Log(LogLevel.Debug, "Delete deals method is over.");
            return Ok(removedDeals);
        }

        private float CountSumByAttributeId(List<Product> products, Deal deal, int attributeId)
            => (float)products.Select(p => p.AttributeValues.First(pr => pr.Attribute.Id == attributeId).NumValue * deal.DealProducts.First(dp => dp.ProductId == p.Id).Count).Sum(val => val);

        private async Task<OrderFormParseResult> ParseOrderForm(IFormFile reqFile)
        {
            _logger.Log(LogLevel.Debug, "Starting parse order form method...");
            OrderFormParseResult parseResult = new OrderFormParseResult();

            _logger.Log(LogLevel.Trace, "Opening and reading the order form file...");
            using (var package = new ExcelPackage(reqFile.OpenReadStream()))
            {
                Dictionary<string, int> skusAndCounts = new Dictionary<string, int>();

                foreach (ExcelWorksheet worksheet in package.Workbook.Worksheets)
                {
                    _logger.Log(LogLevel.Trace, "Getting row counts...");
                    int rowCount = worksheet.Dimension.End.Row;
                    _logger.Log(LogLevel.Trace, "Row counts successfully fetched");

                    _logger.Log(LogLevel.Trace, "Checking TIN...");
                    if (!string.IsNullOrWhiteSpace(parseResult.Tin) && !string.IsNullOrWhiteSpace(worksheet.Cells[TinCellAddress].Value?.ToString()) && parseResult.Tin != worksheet.Cells[TinCellAddress].Value?.ToString())
                    {
                        _logger.Log(LogLevel.Error, $"In the order form funded different TIN values.");
                        throw new Exception("В заказной форме обнаружены разные значения ИНН");
                    }
                    _logger.Log(LogLevel.Trace, "TIN is checked");

                    _logger.Log(LogLevel.Trace, "Filling parse values...");
                    if (string.IsNullOrWhiteSpace(parseResult.Tin)) //Проверяем заполнен ли ИНН
                    {
                        parseResult.Tin = worksheet.Cells[TinCellAddress].Value?.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(parseResult.Rrc))
                    {
                        parseResult.Rrc = worksheet.Cells[RrcCellAddress].Value?.ToString();
                    }

                    if (string.IsNullOrWhiteSpace(parseResult.Brand))
                    {
                        parseResult.Brand = worksheet.Cells[BrandCellAddress].Value?.ToString();
                    }

                    for (int i = 0; i < rowCount - SkuCellRowIndex + 1; i++)
                    {
                        try
                        {
                            var countCellObject = worksheet.Cells[SkuCellRowIndex + i, CountColumnNum].Value;
                            if (countCellObject != null && countCellObject.ToString().Replace(" ", String.Empty) != "")
                            {
                                int countIntValue = int.Parse(worksheet.Cells[SkuCellRowIndex + i, CountColumnNum].Value.ToString());

                                if (countIntValue > 0)
                                    skusAndCounts.Add(worksheet.Cells[SkuCellRowIndex + i, SkuColumnNum].Value.ToString(), countIntValue);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Log(LogLevel.Error, $"Not valid data in row {SkuCellRowIndex + i}. Error: {ex.Message}");
                            throw new InvalidCastException($"Невалидное значение количества в строке {SkuCellRowIndex + i}", ex);
                        }
                    }
                    _logger.Log(LogLevel.Trace, "Parse values are filled");
                }
                parseResult.skusAndCountsDictionary = skusAndCounts;
            }

            var brandListId = int.Parse(_configuration.GetSection("ListIds:Brand").Value);
            var brandListValue = (await _httpPimCommunicator.GetList(brandListId, HttpContext)).ListValues.FirstOrDefault(lv => lv.Value.Trim().ToUpper() == parseResult.Brand.Trim().ToUpper());
            if (brandListValue == null)
                throw new Exception($"Бренд '{parseResult.Brand}' не найден в списке брендов");

            parseResult.BrandId = brandListValue.Id;

            return parseResult;
        }
    }
}