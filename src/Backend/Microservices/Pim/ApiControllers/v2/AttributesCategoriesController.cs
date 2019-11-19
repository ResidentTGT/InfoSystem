using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Pim.Client.v2;
using Company.Pim.Helpers.v2;
using Attribute = Company.Common.Models.Pim.Attribute;
using NLog;

namespace Company.Pim.ApiControllers.v2
{
    [Route("v2/[controller]")]
    public class AttributesCategoriesController : Controller
    {
        private readonly PimContext _context;
        private readonly IWebApiCommunicator _httpWebApiCommunicator;
        private readonly TransformModelHelpers _transformModelHelper;
        private readonly Logger _logger;

        public AttributesCategoriesController(PimContext context, IWebApiCommunicator httpWebApiCommunicator, TransformModelHelpers transformModelHelpers)
        {
            _context = context;
            _httpWebApiCommunicator = httpWebApiCommunicator;
            _transformModelHelper = transformModelHelpers;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int categoryId)
        {          

            _logger.Log(LogLevel.Debug, $"Start getting the category by id='{categoryId}'...");

            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, $"Getting category by id='{categoryId}'...");
            var category = await _context.Categories
                .Include(c => c.AttributeCategories)
                .ThenInclude(ac => ac.Attribute)
                .ThenInclude(a => a.AttributePermissions)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.DeleteTime == null);

            if (category == null)
            {
                _logger.Log(LogLevel.Error, $"Category with id='{categoryId}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Category with id='{categoryId}' successfully received");

            _logger.Log(LogLevel.Trace, $"Getting user roles...");
            var userRoles = user.UserRoles.Select(ur => ur.Role);
            _logger.Log(LogLevel.Trace, $"User roles successfully received");

            _logger.Log(LogLevel.Debug, $"Getting category by id='{categoryId}' is finished");
            return Ok(category.AttributeCategories
                .Where(ac => filterAttributeCmpyUserRole(ac.Attribute, userRoles))
                .Select(ac => _transformModelHelper.TransformAttributeCategory(ac)));
        }

        [HttpGet("byCategoriesAndAttributesIds")]
        public async Task<IActionResult> Get()
        {          
            _logger.Log(LogLevel.Debug, $"Start getting categories...");

            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            var categoriesIds = new List<int>();
            
            if (Request.Query.ContainsKey("categoriesIds") && !string.IsNullOrEmpty(Request.Query["categoriesIds"].ToString()))
            {
                _logger.Log(LogLevel.Trace, "Getting IDs of categories from the request query...");
                categoriesIds = Request.Query["categoriesIds"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"IDs of categories '{String.Join(',', categoriesIds)}' from the request query successfully received");
            }

            var attributesIds = new List<int>();
            if (Request.Query.ContainsKey("attributesIds") && !string.IsNullOrEmpty(Request.Query["attributesIds"].ToString()))
            {
                _logger.Log(LogLevel.Trace, "Getting IDs of attributes from the request query...");
                attributesIds = Request.Query["attributesIds"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"IDs of attributes '{String.Join(',', attributesIds)}' from the request query successfully received");
            }

            _logger.Log(LogLevel.Trace, "Getting attribute categories...");
            var attributeCategories = await _context.AttributeCategories
                .Include(ac => ac.Attribute)
                .ThenInclude(a => a.AttributePermissions)
                .Where(ac => categoriesIds.Contains(ac.CategoryId) && attributesIds.Contains(ac.AttributeId)).ToListAsync();
            _logger.Log(LogLevel.Trace, $"Attribute categories successfully received");

            _logger.Log(LogLevel.Trace, $"Getting user roles...");
            var userRoles = user.UserRoles.Select(ur => ur.Role);
            _logger.Log(LogLevel.Trace, $"User roles successfully received");

            _logger.Log(LogLevel.Debug, $"Getting categories is finished");
            return Ok(attributeCategories
                .Where(ac => filterAttributeCmpyUserRole(ac.Attribute, userRoles))
                .Select(ac => _transformModelHelper.TransformAttributeCategory(ac)));
        }

        [HttpPut("{categoryId}")]
        public async Task<IActionResult> Edit([FromRoute] int categoryId, [FromBody] List<AttributeCategory> categoryAttributes)
        {
            _logger.Log(LogLevel.Debug, $"Start editing category with id='{categoryId}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Error, "Model state is not valid");
                return BadRequest(ModelState);
            }          

            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, $"Getting user by userId='{userId}'...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, $"Getting category by id='{categoryId}'...");
            var category = _context.Categories
                .Include(c => c.AttributeCategories)
                .ThenInclude(ac => ac.Attribute)
                .ThenInclude(a => a.AttributePermissions)
                .FirstOrDefault(c => c.Id == categoryId && c.DeleteTime == null);

            if (category == null)
            {
                _logger.Log(LogLevel.Error, $"Category with id='{categoryId}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Category with id='{categoryId}' successfully received");

            _logger.Log(LogLevel.Trace, $"Getting user roles...");
            var userRoles = user.UserRoles.Select(ur => ur.Role);
            _logger.Log(LogLevel.Trace, $"User roles successfully received");

            _logger.Log(LogLevel.Trace, $"Getting category attributes entities...");
            var categoryAttributesEntities = category.AttributeCategories
                                                     .Where(ac => filterAttributeCmpyUserRole(ac.Attribute, userRoles))
                                                     .ToDictionary(ac => ac.AttributeId);
            _logger.Log(LogLevel.Trace, $"Category attributes entities successfully received");

            _logger.Log(LogLevel.Trace, $"Getting attributes with permissions...");
            var categoryAttributesIds = categoryAttributes.Select(ac => ac.AttributeId);
            var attributesWithPermissions = (await _context.Attributes.Include(a => a.AttributePermissions)
                                                           .Where(a => a.DeleteTime == null && categoryAttributesIds.Contains(a.Id))
                                                           .ToListAsync())
                                                           .Where(a => filterAttributeCmpyUserRole(a, userRoles))
                                                           .ToDictionary(a => a.Id);
            _logger.Log(LogLevel.Trace, $"Attributes('{String.Join(',', categoryAttributesIds)}') with permissions successfully received");

            _logger.Log(LogLevel.Trace, $"Getting filtered category attributes...");
            var filteredCategoryAttributes = categoryAttributes.Where(ac => attributesWithPermissions.ContainsKey(ac.AttributeId)).ToList();
            _logger.Log(LogLevel.Trace, $"Filtered category attributes successfully received");

            _logger.Log(LogLevel.Trace, $"Updating filtered category attributes...");
            foreach (var newAttribute in filteredCategoryAttributes)
            {
                if (categoryAttributesEntities.ContainsKey(newAttribute.AttributeId))
                {
                    categoryAttributesEntities[newAttribute.AttributeId].IsRequired = newAttribute.IsRequired;
                    categoryAttributesEntities[newAttribute.AttributeId].ModelLevel = newAttribute.ModelLevel;
                    categoryAttributesEntities[newAttribute.AttributeId].IsFiltered = newAttribute.IsFiltered;
                    categoryAttributesEntities[newAttribute.AttributeId].IsVisibleInProductCard = newAttribute.IsVisibleInProductCard;
                    categoryAttributesEntities[newAttribute.AttributeId].IsKey = newAttribute.IsKey;
                }
                else
                {
                    category.AttributeCategories.Add(newAttribute);
                }
            }
            _logger.Log(LogLevel.Trace, $"Filtered category attributes are updated");

            _logger.Log(LogLevel.Trace, $"Getting attributes to delete...");
            var attributesToDelete = category.AttributeCategories.Where(ac => !filteredCategoryAttributes.Select(fca => fca.AttributeId).Contains(ac.AttributeId)).ToList();
            _logger.Log(LogLevel.Trace, $"Attributes to delete successfully received");

            _logger.Log(LogLevel.Trace, $"Removing attributes...");
            _context.RemoveRange(attributesToDelete);
            _logger.Log(LogLevel.Trace, $"Attributes are removed");           

            try
            {
                _logger.Log(LogLevel.Trace, "Saving categories changes to database...");
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error to save categories values. Error: {e.Message}");
                return BadRequest(e.Message);
            }
            _logger.Trace("Categories was successfully saved");

            _logger.Log(LogLevel.Debug, $"Editing category with id='{categoryId}' is finished");
            return Ok(category.AttributeCategories.Select(ac => _transformModelHelper.TransformAttributeCategory(ac)));
        }

        private Func<Attribute, IEnumerable<Role>, bool> filterAttributeCmpyUserRole = (attribute, userRoles) => userRoles.Any(r => attribute.AttributePermissions.Select(ap => ap.RoleId).Contains(r.Id));
    }
}