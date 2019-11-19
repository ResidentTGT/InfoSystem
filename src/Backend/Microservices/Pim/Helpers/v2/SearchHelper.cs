using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NLog;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Pim.Client.v2;
using Company.Pim.Options;

namespace Company.Pim.Helpers.v2
{
    public class SearchHelper
    {
        private readonly IWebApiCommunicator _webApiCommunicator;
        private readonly ISeasonsMsCommunicator _seasonsMsCommunicator;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly PimContext _context;
        private readonly CurrentSeasonOptions _currentSeasonOptions;
        private readonly Logger _logger;
        private const string SkuParametr = "SKU";
        private const string NameParametr = "НАИМЕНОВАНИЕ";
        private const string ImportsParametr = "ИМПОРТ";

        public SearchHelper(IWebApiCommunicator webApiCommunicator,
            ISeasonsMsCommunicator seasonsMsCommunicator,
            TreeObjectHelper treeObjectHelper,
            IOptions<CurrentSeasonOptions> currentSeasonOptions,
            IHttpContextAccessor httpContextAccessor,
            PimContext context)
        {
            _webApiCommunicator = webApiCommunicator;
            _seasonsMsCommunicator = seasonsMsCommunicator;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _currentSeasonOptions = currentSeasonOptions.Value;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public IQueryable<Product> SearchProductsQuery(bool withoutCategory, List<int> categoriesIds, string searchString)
        {
            var searchStr = searchString != null ? JsonConvert.DeserializeObject<string[]>(searchString) : null;
            var search = FillingSearchStruct(searchStr);
            var sqlInnerRequests = GenerateInnerRequest(search, categoriesIds, withoutCategory, "", "", "", 1.0f);
            sqlInnerRequests.RemoveAll(s => s == "");
            var productsHeaderRequest = GenerateHeaderRequest("*", "rangeSizeProductId");
            return _context.Products.FromSql($"{productsHeaderRequest} Where {string.Join(" and ", sqlInnerRequests)}" + $") result Group by result.\"Id\", result.\"NameOrigEng\" ) superresult) and \"DeleteTime\" is null");
        }

        private Search FillingSearchStruct(string[] searchStr)
        {
            _logger.Log(LogLevel.Info, "Start filling search struct...");
            Search search = new Search();
            try
            {
                if (searchStr == null)
                    return search;

                foreach (string str in searchStr)
                {
                    if (!String.IsNullOrEmpty(str))
                    {
                        var strTemp = str.Split(new char[] {':'}, 2);
                        if (strTemp.Count() > 1 && !String.IsNullOrEmpty(strTemp[0]))
                        {
                            if (strTemp[0].ToUpper().Contains(SkuParametr))
                            {
                                search.sku = strTemp[1].Trim().ToUpper().Split(new string[] {"||"}, StringSplitOptions.None).ToList();
                            }
                            else if (strTemp[0].ToUpper().Contains(ImportsParametr))
                            {
                                var strArray = strTemp[1].Trim().Split(new string[] {"||"}, StringSplitOptions.None);
                                bool nullFlag = false;
                                string[] strImports = null;

                                if (strArray.Contains(""))
                                {
                                    nullFlag = true;
                                    strImports = strArray.Where(v => !string.IsNullOrWhiteSpace(v)).ToArray();
                                }

                                if (strImports != null)
                                {
                                    search.imports = strImports.Select(ch => ToNullableInt(ch)).ToList();
                                }
                                else
                                {
                                    search.imports = strArray.Select(ch => ToNullableInt(ch)).ToList();
                                }

                                if (nullFlag)
                                    search.imports.Add(null);
                            }
                            else if (strTemp[0].ToUpper().Contains(NameParametr))
                            {
                                search.names = strTemp[1].Trim().ToUpper().Split(new string[] {"||"}, StringSplitOptions.None).ToList();
                            }
                            else
                            {
                                search.attrParams.Add(
                                    new AttrParams()
                                    {
                                        name = strTemp[0].Trim().ToUpper(),
                                        values = strTemp[1].Trim().ToUpper().Split(new string[] {"||"}, StringSplitOptions.None).ToList()
                                    }
                                );
                            }
                        }
                        else
                        {
                            search.unnameds.Add(
                                new AttrParams()
                                {
                                    values = strTemp[0].Trim().ToUpper().Split(new string[] {"||"}, StringSplitOptions.None).ToList()
                                }
                            );
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't parse search string. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Info, "Filling search struct is finished");
            return search;
        }

        private async Task<float> GetCurrentExchangeCoefficientAsync(string currency)
        {
            var policyExchangeRates = (await _seasonsMsCommunicator.GetDiscountPolicyAsync(_currentSeasonOptions.Id, _httpContextAccessor.HttpContext)).ExchangeRates.OrderByDescending(ex => ex.CreateTime).FirstOrDefault();

            var coefExchange = 1.0f;

            if (policyExchangeRates != null)
            {
                if (currency == "USD")
                {
                    if (policyExchangeRates.EurUsd.HasValue)
                        coefExchange = policyExchangeRates.EurUsd.Value;
                    else
                        _logger.Error($"There are no EurUsd coefficient in current season (seasonId: {_currentSeasonOptions.Id})");
                }
                else if (currency == "RUB")
                {
                    if (policyExchangeRates.EurRub.HasValue)
                        coefExchange = policyExchangeRates.EurRub.Value;
                    else
                        _logger.Error($"There are no EurRub coefficient in current season (seasonId: {_currentSeasonOptions.Id})");
                }
            }

            return coefExchange;
        }

        private async Task<IQueryable<Product>> GetProductsPagedResultAsync(Search search, User user, List<int> attributesIds, int? pageSize, int? pageNumber,
            List<int> categoryIds, bool withoutCategory, string sortByPropertyName, bool sortByAsc,
            string ageMonthRange, string ageYearRange, string priceRange, float coefExchange = 1)
        {
            _logger.Log(LogLevel.Info, "Start filling search struct...");

            _logger.Trace("Generating inner request...");
            var sqlInnerRequests = GenerateInnerRequest(search, categoryIds, withoutCategory, ageMonthRange, ageYearRange, priceRange, coefExchange);
            _logger.Trace("Inner request is generated");

            _logger.Trace("Generating count header request...");
            var countHeaderRequest = GenerateHeaderRequest("*", "\"ProductId\"");
            _logger.Trace("Count header request is generated");

            _logger.Trace("Generating products header request...");
            var productsHeaderRequest = GenerateHeaderRequest("*", "rangeSizeProductId");
            _logger.Trace("Products header request is generated");

            var skipTakeRequest = (pageSize != null && pageNumber != null)
                ? $"offset {pageSize * pageNumber} limit {pageSize}"
                : "";

            var watch = System.Diagnostics.Stopwatch.StartNew();
            _logger.Trace("Getting count query...");
            var countQuery = _context.Products.FromSql(countHeaderRequest + $"Where {string.Join(" and ", sqlInnerRequests)}" + $") result Group by result.\"Id\", result.\"NameOrigEng\") superresult) and \"DeleteTime\" is null");
            _logger.Log(LogLevel.Trace, $"Count query successfully received");
            watch.Stop();
            _logger.Debug($"Get count time: {watch.Elapsed.ToString()}");

            _logger.Trace("Getting products query...");
            var query = _context.Products.FromSql(productsHeaderRequest + $"Where {string.Join(" and ", sqlInnerRequests)}" + $") result Group by result.\"Id\", result.\"NameOrigEng\" {skipTakeRequest}) superresult) and \"DeleteTime\" is null");
            _logger.Log(LogLevel.Trace, $"Products query successfully received");

            // Достаем продукты, по найденным Id, подгружая ParentProducts
            watch.Reset();
            watch.Start();
            _logger.Log(LogLevel.Trace, $"Products query and count query by category ids successfully received");

            _logger.Trace("Including data to products query...");
            query = query
                    .Include(p => p.ParentProduct.ParentProduct)
                    .Include(p => p.SubProducts)
                    .ThenInclude(sp => sp.SubProducts)
                ;
            _logger.Trace("Including data to products is finished");

            return query;
        }

        private List<string> GetListStringRangesFromStringOfRanges(string ranges, string conditionString)
        {
            var sqlRangesConditions = new List<string>();

            if (!String.IsNullOrEmpty(ranges))
            {
                foreach (var rangeValue in ranges.Split(","))
                {
                    var from = String.IsNullOrEmpty(rangeValue.Split("-").FirstOrDefault()) ? "NULL" : rangeValue.Split("-").FirstOrDefault();
                    var to = String.IsNullOrEmpty(rangeValue.Split("-").LastOrDefault()) ? "NULL" : rangeValue.Split("-").LastOrDefault();

                    sqlRangesConditions.Add($"(int4range({from}, {to}) {conditionString})");
                }
            }

            return sqlRangesConditions;
        }

        private List<string> GenerateInnerRequest(Search search, List<int> categoryIds, bool withoutCategory, string ageMonthRange, string ageYearRange, string priceRange, float coefExchange)
        {
            _logger.Log(LogLevel.Info, "Start generating inner request...");
            var sqlInnerRequests = new List<string>();

            sqlInnerRequests.Add("pr.\"ParentId\" is null");

            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(priceRange, $"@> (pr.\"BwpMin\"/{coefExchange}) :: int4")));
            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(ageMonthRange, "&& colorR.ageMonthRange")));
            sqlInnerRequests.Add(String.Join(" Or ", GetListStringRangesFromStringOfRanges(ageYearRange, "&& colorR.ageYearRange")));

            _logger.Log(LogLevel.Trace, $"Adding categories({String.Join(", ", categoryIds)}...) and 'withoutCategories' = '{withoutCategory.ToString()}' to search sql script...");
            var categoriesRequestsList = new List<string>();
            if (categoryIds.Any())
                categoriesRequestsList.Add($"colorR.rangesizeCategoryId in ({String.Join(", ", categoryIds.ToList())})");

            if (withoutCategory)
                categoriesRequestsList.Add("colorR.rangesizeCategoryId is null");

            if (categoriesRequestsList.Any())
                sqlInnerRequests.Add(String.Join(" or ", categoriesRequestsList));
            _logger.Log(LogLevel.Trace, $"Categories to search sql script is added");

            if (search.imports.Any())
            {
                _logger.Log(LogLevel.Info, $"Adding imports('{String.Join(", ", search.imports.Where(i => i != null))}') to search sql script...");
                var importsRequestsList = new List<string>();

                if (search.imports.Any(i => i != null))
                    importsRequestsList.Add($"colorR.rangesizeImportId in ({String.Join(", ", search.imports.Where(i => i != null).ToList())})");

                if (search.imports.Any(i => i == null))
                    importsRequestsList.Add("colorR.rangesizeImportId is null");

                sqlInnerRequests.Add(String.Join(" or ", importsRequestsList));
                _logger.Log(LogLevel.Info, $"Imports('{String.Join(", ", search.imports.Where(i => i != null))}') to search sql script is added");
            }

            foreach (var s in search.sku)
            {
                _logger.Log(LogLevel.Info, $"Adding sku='{s}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSmartSearch) SIMILAR TO '%{s}%' " +
                                     $"or text(colorR.colorModelSmartSearch) SIMILAR TO '%{s}%' " +
                                     $"or text(pr.\"SmartSearchArray\") SIMILAR TO '%{s}%')");
                _logger.Log(LogLevel.Info, $"Sku='{s}' to search sql script is added");
            }

            foreach (var s in search.names)
            {
                _logger.Log(LogLevel.Info, $"Adding name='{s}' to search sql script...");
                sqlInnerRequests.Add("UPPER(pr.\"Name\") Like '%" + s + "%'");
                _logger.Log(LogLevel.Info, $"Name='{s}' to search sql script is added");
            }

            foreach (var param in search.unnameds)
            {
                _logger.Log(LogLevel.Info, $"Adding unnamed parametr='{param.name}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSmartSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%' " +
                                     $"or text(colorR.colorModelSmartSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%' " +
                                     $"or text(pr.\"SmartSearchArray\") SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? "NULL" : $"{v}"; return v; }).ToList())})%')");
                _logger.Log(LogLevel.Info, $"Unnamed parametr='{param}' to search sql script is added");
            }

            foreach (var param in search.attrParams)
            {
                _logger.Log(LogLevel.Info, $"Adding attr parametr='{param.name}' to search sql script...");
                sqlInnerRequests.Add($"(text(colorR.rangesizeSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%' " +
                                     $"or text(colorR.colorModelSearch) SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%' " +
                                     $"or text(pr.\"FullSearchArray\") SIMILAR TO '%({String.Join($"|", param.values.Select(v => { v = v == "" ? $"{param.name}:NULL" : $"{param.name}:{v}"; return v; }).ToList())})%')");
                _logger.Log(LogLevel.Info, $"Attr parametr='{param}' to search sql script is added");
            }

            _logger.Log(LogLevel.Info, "Generating inner request is finished");
            return sqlInnerRequests;
        }

        private string GenerateHeaderRequest(string outputParameters, string aggregatedIds)
            => $"select {outputParameters} from public.\"Products\" prod " +
               "where prod.\"Id\" in ( " +
               "select unnest(ids) from( " +
               $"select result.\"Id\", array_agg(result.{aggregatedIds}) as ids, result.\"NameOrigEng\" from( " +
               "select pr.\"Id\", pr.\"BwpMin\", pr.\"NameOrigEng\", pr.\"ProductId\", pr.\"ParentId\", pr.\"FullSearchArray\", pr.\"SmartSearchArray\", colorR.rangesizeCategoryId, colorR.rangesizeImportId, colorR.rangeSizeProductId, colorR.rangesizeSearch, colorR.colorModelSearch, colorR.colorModelSmartSearch, colorR.rangesizeSmartSearch, colorR.ageMonthRange, colorR.ageYearRange " +
               "from public.\"ProductSearch\" pr " +
               "left join( " +
               "select color.\"Id\" as colorId, color.\"ParentId\" as colorParentId, rangesize.\"ImportId\" as rangesizeImportId, rangesize.\"CategoryId\" as rangesizeCategoryId, rangesize.\"ProductId\" as rangeSizeProductId, color.\"FullSearchArray\" as colorModelSearch, rangesize.\"FullSearchArray\" as rangesizeSearch, rangesize.\"AgeMonthRange\" as ageMonthRange, rangesize.\"AgeYearRange\" as ageYearRange, color.\"SmartSearchArray\" as colorModelSmartSearch, rangesize.\"SmartSearchArray\" as rangesizeSmartSearch from public.\"ProductSearch\" color " +
               "left join( " +
               "select \"Id\", \"FullSearchArray\", \"SmartSearchArray\", \"ParentId\" as pId, \"ProductId\", \"AgeMonthRange\", \"AgeYearRange\", \"ImportId\", \"CategoryId\" from public.\"ProductSearch\" " +
               ") rangesize on rangesize.pId = color.\"Id\" " +
               ") colorR on pr.\"Id\" = colorR.colorParentId ";

        private int? ToNullableInt(string s) => int.TryParse(s, out int i) ? (int?) i : null;
    }
}