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
using NLog;

namespace Company.Pim.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/[controller]")]
    public class CategoriesController : Controller
    {
        private readonly PimContext _context;
        private IWebApiCommunicator _httpWebApiCommunicator { get; set; }
        private TransformModelHelpers _transformModelHelper { get; set; }
        private readonly Logger _logger;

        public CategoriesController(PimContext context, IWebApiCommunicator httpWebApiCommunicator, TransformModelHelpers transformModelHelpers)
        {
            _context = context;
            _transformModelHelper = transformModelHelpers;
            _httpWebApiCommunicator = httpWebApiCommunicator;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            _logger.Log(LogLevel.Debug, $"Start getting category by id='{id}'...");

            _logger.Log(LogLevel.Trace, $"Getting category by id='{id}'...");
            var category = await _context.Categories.FindAsync(id);
            
            if (category == null)
            {
                _logger.Log(LogLevel.Error, $"Category with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Category '{category.Name}' successfully received");

            _logger.Log(LogLevel.Debug, "Getting category by id is finished.");
            return Ok(category);
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.Log(LogLevel.Debug, "Start getting categories...");

            _logger.Log(LogLevel.Trace, "Getting categories...");
            var categories = _context.Categories.Where(c => c.DeleteTime == null);
            _logger.Log(LogLevel.Trace, "Categories successfully received");

            _logger.Log(LogLevel.Trace, "Loading categories...");
            await categories.LoadAsync();
            _logger.Log(LogLevel.Trace, "Load categories is completed");

            _logger.Log(LogLevel.Debug, "Getting categories is finished.");
            return Ok(await categories.Where(c => c.DeleteTime == null && c.ParentId == null)
                .OrderBy(c => c.Name)
                .Select(c => _transformModelHelper.TransformCategory(c, true))
                .ToArrayAsync());
        }

        [HttpGet("attributes")]
        public async Task<IActionResult> GetAttributeCmpyCategoriesIdsFromCalculator()
        {
            _logger.Log(LogLevel.Debug, "Start getting attribute categories from calculator...");
            var ids = new List<int>();

            _logger.Log(LogLevel.Trace, "Getting ids from request query...");
            if (Request.Query.ContainsKey("ids") && !String.IsNullOrEmpty(Request.Query["ids"].ToString()))
            {
                ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"Ids '{String.Join(',', ids)}' successfully received");
            }
          
            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "userId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, "Getting user by userId...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, "Getting attribute categories...");
            var attributeCategories = ids.Any() ? _context.AttributeCategories.Where(ac => ids.Contains(ac.CategoryId)) : _context.AttributeCategories;
            _logger.Log(LogLevel.Trace, "Attribute categories successfully received");

            _logger.Log(LogLevel.Debug, "Getting attribute categories from calculator is finished.");
            return Ok(await attributeCategories.Select(a => a.Attribute)
                .Distinct()
                .Where(
                    a => user.UserRoles.Select(ur => ur.Role).Any(
                        r => a.AttributePermissions
                            .Where(ap => ap.Value > DataAccessMethods.Read)
                            .Select(ap => ap.RoleId)
                            .Contains(r.Id)
                    )

                )
                .OrderBy(c => c.Name)
                .Select(ad => _transformModelHelper.TransformAttribute(ad, false, true))
                .ToListAsync());
        }

        [HttpGet("{id}/attributes")]
        public async Task<IActionResult> GetAttributes(int id)
        {
            _logger.Log(LogLevel.Debug, $"Start getting attributes of category wit id='{id}'...");
           
            var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "userId");
            User user = null;

            try
            {
                _logger.Log(LogLevel.Trace, "Getting user by userId...");
                var response = _httpWebApiCommunicator.GetUser(Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, "Deserializing user response data...");
                user = JsonConvert.DeserializeObject<User>(await response.Result.Content.ReadAsStringAsync());
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, $"Getting category by id='{id}'...");
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                _logger.Log(LogLevel.Error, $"Category with id='{id}' not found");
                return null;
            }
            _logger.Log(LogLevel.Trace, $"Category '{category.Name}' successfully received");

            _logger.Log(LogLevel.Debug, "Getting attribute categories is finished.");
            return Ok(await _context.AttributeCategories.Where(ac => ac.CategoryId == id)
                .Select(c => c.Attribute)
                .Include(a => a.AttributePermissions)
                .Where(
                    a => user.UserRoles.Select(ur => ur.Role).Any(
                        r => a.AttributePermissions.Select(ap => ap.RoleId)
                            .Contains(r.Id)
                    )

                )
                .OrderBy(a => a.Name)
                .Select(ad => _transformModelHelper.TransformAttribute(ad, false, true))
                .ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            _logger.Log(LogLevel.Debug, "Start creating category...");
            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, $"Checking category '{category.Name}'...");
            var categoryEntity = await _context.Categories.FirstOrDefaultAsync(c => c.Name == category.Name && c.ParentId == category.ParentId && c.DeleteTime == null);
            if (categoryEntity == null)
            {
                _logger.Log(LogLevel.Trace, $"Category '{category.Name}' is checked");

                _logger.Log(LogLevel.Trace, "Getting parent category...");
                var parentCategory = await _context.Categories.Include(x => x.AttributeCategories).FirstOrDefaultAsync(c => c.Id == category.ParentId);
                _logger.Log(LogLevel.Trace, parentCategory == null ? "Category is root, parent category not found" : $"Parent category '{parentCategory.Name}' successfully received");

                _logger.Log(LogLevel.Trace, $"Updating the '{category.Name}' category data...");
                category.SubCategories = new List<Category>();
                category.CreateTime = DateTime.Now;
                category.CreatorId = userId;
                category.AttributeCategories = parentCategory?.AttributeCategories.Select(pc => new AttributeCategory
                {
                    AttributeId = pc.AttributeId,
                    ModelLevel = pc.ModelLevel,
                    IsRequired = pc.IsRequired
                }).ToList();

                _context.Categories.Add(category);               

                _logger.Log(LogLevel.Trace, $"Saving category '{category.Name}' to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{category.Name}' category. Error: {e.Message}");
                    return BadRequest($"Error on save '{category.Name}' category");
                }
                _logger.Log(LogLevel.Trace, $"Category '{category.Name}' was successfully saved");

                _logger.Log(LogLevel.Debug, "Creating category is finished.");
                return Ok(category);
            }
            else
            {
                _logger.Log(LogLevel.Error, $"Category with same 'Name': '{category.Name}' and 'ParentId' already exist.");
                return BadRequest($"Category with same 'Name': '{category.Name}' and 'ParentId' already exist.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Category category)
        {
            _logger.Log(LogLevel.Debug, "Start editing category...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, $"Getting category by id='{id}'...");
            var categoryEntity = await _context.Categories.FindAsync(id);

            if (categoryEntity == null)
            {
                _logger.Log(LogLevel.Error, $"Category with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Category with id='{id}' successfully received");

            _logger.Log(LogLevel.Trace, $"Checking category '{categoryEntity.Name}'...");
            if (!_context.Categories.Any(c => c.Name == category.Name && c.ParentId == category.ParentId && c.DeleteTime == null))
            {
                _logger.Log(LogLevel.Trace, $"Category '{categoryEntity.Name}' is checked");

                _logger.Log(LogLevel.Trace, $"Updating the '{categoryEntity.Name}' category data...");
                categoryEntity.Name = category.Name;
                categoryEntity.ParentId = category.ParentId;
                _logger.Log(LogLevel.Trace, $"'{categoryEntity.Name}' category data is updated");
               
                _logger.Log(LogLevel.Trace, $"Saving '{categoryEntity.Name}' category to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{categoryEntity.Name}' category. Error: {e.Message}");
                    return BadRequest($"Error on saving '{categoryEntity.Name}' category");
                }
                _logger.Log(LogLevel.Trace, $"Category '{categoryEntity.Name}' was successfully saved");

                _logger.Log(LogLevel.Debug, "Editing category is finished.");
                return Ok(_transformModelHelper.TransformCategory(categoryEntity));
            }
            else
            {
                _logger.Log(LogLevel.Error, $"Category with same 'Name': '{category.Name}' and 'ParentId' already exist.");
                return BadRequest($"Category with same 'Name': '{category.Name}' and 'ParentId' already exist.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.Log(LogLevel.Debug, $"Start deleting category with id='{id}'...");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            try
            {
                _logger.Log(LogLevel.Debug, $"Removing category by id='{id}'...");
                var category = RemoveCategory(id, userId);
                _logger.Log(LogLevel.Debug, $"Category with id='{id}' is removed");
               
                _logger.Log(LogLevel.Trace, $"Saving '{category.Name}' category to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{category.Name}' category. Error: {e.Message}");
                    return BadRequest($"Error on save '{category.Name}' category");
                }
                _logger.Log(LogLevel.Trace, $"'{category.Name}' category was successfully saved");

                _logger.Log(LogLevel.Debug, "Deleting category is finished.");
                return Ok(_transformModelHelper.TransformCategory(category));
            }
            catch (Exception e)
            {               
                _logger.Log(LogLevel.Error, $"Couldn't remove category with id='{id}'. Error: {e.Message}");
                return NotFound();
            }
        }

        private Category RemoveCategory(int categoryId, int userId)
        {
            _logger.Log(LogLevel.Trace, $"Start removing category with id='{categoryId}'...");
            var category = _context.Categories.Include(c => c.SubCategories).First(c => c.Id == categoryId);
            _logger.Log(LogLevel.Trace, $"Category with id='{categoryId}' successfully received");

            _logger.Log(LogLevel.Trace, $"Updating category with id='{categoryId}'...");
            category.DeleterId = userId;
            category.DeleteTime = DateTime.Now;
            _logger.Log(LogLevel.Trace, $"Category with id='{categoryId}' are updated");

            _logger.Log(LogLevel.Debug, $"Removing attribute category of the category with id='{categoryId}'...");
            _context.AttributeCategories.RemoveRange(_context.AttributeCategories.Where(ac => ac.CategoryId == categoryId));
            _logger.Log(LogLevel.Debug, "Attribute category is removed");

            _logger.Log(LogLevel.Trace, $"Updating products of the category with id='{categoryId}'...");
            _context.Products.Where(p => p.CategoryId == category.Id).ToList().ForEach(p => p.CategoryId = null);
            _logger.Log(LogLevel.Trace, "Products are updated");

            _logger.Log(LogLevel.Debug, $"Removing sub category of the category with id='{categoryId}'...");
            foreach (var subCategory in category.SubCategories)
            {
                RemoveCategory(subCategory.Id, userId);
            }
            _logger.Log(LogLevel.Debug, "Sub category is removed");

            return category;
        }
    }
}