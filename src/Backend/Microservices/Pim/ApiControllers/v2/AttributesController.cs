using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using Company.Common.Extensions;
using Company.Common.Models.Identity;
using Company.Common.Models.Pim;
using Company.Pim.Client.v2;
using Company.Pim.Helpers.v2;
using Attribute = Company.Common.Models.Pim.Attribute;
using NLog;

namespace Company.Pim.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/[controller]")]
    public class AttributesController : Controller
    {
        private readonly PimContext _context;
        private readonly Logger _logger;
        private IWebApiCommunicator _httpWebApiCommunicator { get; set; }
        private TransformModelHelpers _transformModelHelper { get; set; }       

        public AttributesController(PimContext context, IWebApiCommunicator httpWebApiCommunicator, TransformModelHelpers transformModelHelpers)
        {
            _context = context;
            _httpWebApiCommunicator = httpWebApiCommunicator;
            _transformModelHelper = transformModelHelpers;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] bool withCategories, [FromQuery] bool withPermissions)
        {           
            _logger.Log(LogLevel.Debug, "Start getting the attributes ...");

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
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Debug, "Getting attributes is finished.");
	        var attributes = _context.Attributes.Where(a => a.DeleteTime == null)
                .Where(
                    a => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id)
                        .Any(rid =>
                            a.AttributePermissions.Select(ap => ap.RoleId).Contains(rid)
                        )
                );

            if (withPermissions)
                attributes = attributes.Include(a => a.AttributePermissions);

            if (withCategories)
                attributes = attributes.Include(a => a.AttributeCategories);

            return Ok(attributes
                .Select(a => _transformModelHelper.TransformAttribute(a, withCategories, withPermissions))
                .ToArray()
                .OrderBy(a => a.Name));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {          
            _logger.Log(LogLevel.Debug, $"Start getting the attributes by id='{id}'...");

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

            _logger.Log(LogLevel.Trace, $"Getting attribute by id='{id}'...");
            var attribute = await _context.Attributes.Where(a => a.DeleteTime == null).Include(a => a.AttributeCategories).Include(a => a.AttributePermissions).FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute with id='{id}' not found...");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute '{attribute.Name}' successfully received");

            _logger.Log(LogLevel.Trace, "Checking permissions...");
            if (!attribute.AttributePermissions.Any(p => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id).Contains(p.RoleId)))
            {
                return Forbid();
            }
            _logger.Log(LogLevel.Trace, "Permissions are checked");

            _logger.Log(LogLevel.Debug, "Getting attributes by id is finished.");
            return Ok(new { Attribute = _transformModelHelper.TransformAttribute(attribute, true), WithCategories = true });
        }

        [HttpGet("groups")]
        public async Task<IActionResult> GetGroups()
        {
            return Ok(await _context.AttributeGroups
                .Where(ag => ag.DeleteTime == null)
                .OrderBy(ag => ag.Name)
                .ToArrayAsync());
        }

        [HttpGet("groups/{id}")]
        public async Task<IActionResult> GetGroup([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Start getting attribute group by id='{id}'...");

            _logger.Log(LogLevel.Trace, $"Getting attribute group by id='{id}'...");
            var attributeGroup = await _context.AttributeGroups.FindAsync(id);
           
            if (attributeGroup == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute group with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute group '{attributeGroup.Name}' successfully received");

            _logger.Log(LogLevel.Debug, "Getting attribute group by id is finished.");
            return Ok(attributeGroup);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attribute attribute)
        {
            _logger.Log(LogLevel.Debug, "Start creating attribute...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }
            
            if (!_context.Attributes.Any(a => a.Name == attribute.Name && a.DeleteTime == null))
            {               

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
                    _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
                }

                _logger.Log(LogLevel.Trace, $"Filling '{attribute.Name}' attribute fields...");
                attribute.CreatorId = user.Id;
                attribute.CreateTime = DateTime.Now;

                _logger.Log(LogLevel.Trace, "Adding attribute permissions...");
                foreach (var role in user.UserRoles.Select(ur => ur.Role))
                {
                    if (attribute.AttributePermissions.All(ap => ap.RoleId != role.Id))
                    {
                        attribute.AttributePermissions.Add(new AttributePermission() { RoleId = role.Id, Value = DataAccessMethods.Read | DataAccessMethods.Write });
                    }
                }
                _logger.Log(LogLevel.Trace, "Attribute permissions are added");
                _logger.Log(LogLevel.Trace, "Fields of attribute values are filled");
               
                _context.Attributes.Add(attribute);

                _logger.Log(LogLevel.Trace, $"Saving attribute '{attribute.Name}' to database...");
                try
                {                    
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving attribute Error: {e.Message}");
                    return BadRequest($"Error on saving attribute.");
                }
                _logger.Log(LogLevel.Trace, "Attribute was successfully saved");

                _logger.Log(LogLevel.Debug, "Creating attribute is finished.");
                return Ok(new { Attribute = _transformModelHelper.TransformAttribute(attribute, true), WithCategories = true });
            }
            else
            {
                _logger.Log(LogLevel.Info, "Attribute already exist");
                return BadRequest($"Attribute with same 'Name': '{attribute.Name}' already exist.");
            }
        }

        [HttpPost("groups")]
        public async Task<IActionResult> CreateGroup([FromBody] AttributeGroup attributeGroup)
        {
            _logger.Log(LogLevel.Debug, "Start creating attribute group...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }            

            if (!_context.AttributeGroups.Any(ag => ag.Name == attributeGroup.Name && ag.DeleteTime == null))
            {
                _logger.Log(LogLevel.Trace, $"Filling '{attributeGroup.Name}' attribute group fields...");
                var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

                attributeGroup.CreatorId = userId;
                attributeGroup.CreateTime = DateTime.Now;

                _context.AttributeGroups.Add(attributeGroup);

                _logger.Log(LogLevel.Trace, $"Saving '{attributeGroup.Name}' attribute group to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{attributeGroup.Name}' attribute group Error: {e.Message}");
                    return BadRequest("Error on saving attribute group.");
                }
                _logger.Log(LogLevel.Trace, $"'{attributeGroup.Name}' attribute group was successfully saved");
                
                _logger.Log(LogLevel.Debug, "Creating attribute group is finished.");
                return Ok(attributeGroup);
            }
            else
            {
                _logger.Log(LogLevel.Info, $"'{attributeGroup.Name}' attribute group already exist");
                return BadRequest($"AttributeGroup with same 'Name': '{attributeGroup.Name}' already exist.");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id, [FromBody] Attribute attribute)
        {
            _logger.Log(LogLevel.Debug, $"Start editing attribute with id='{id}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }           
           
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
                _logger.Log(LogLevel.Error, $"Couldn't get user. Error: {e.Message}");
            }

            _logger.Log(LogLevel.Trace, "Checking permissions...");
            if (!attribute.AttributePermissions.Any(p => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id).Contains(p.RoleId) && p.Value > DataAccessMethods.Read))
            {
                return Forbid();
            }
            _logger.Log(LogLevel.Trace, "Permissions are checked...");

            _logger.Log(LogLevel.Trace, "Getting attribute...");
            var attributeEntity = await _context.Attributes.Include(a => a.AttributeCategories).Include(a => a.AttributePermissions).FirstOrDefaultAsync(a => a.Id == id);
            if (attributeEntity == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute '{attributeEntity.Name}' successfully received");

            if (!_context.Attributes.Any(a => a.Id != id && a.Name == attribute.Name && a.DeleteTime == null))
            {
                try
                {
                    await ChangeAttributeType(attributeEntity, attribute);
                }
                catch (Exception)
                {
                    return BadRequest($"Error on convert value from type {Enum.GetName(typeof(AttributeType), attributeEntity.Type)} to {Enum.GetName(typeof(AttributeType), attribute.Type)}");
                }
                attributeEntity.Name = attribute.Name;
                
                attributeEntity.Type = attribute.Type;
                attributeEntity.MaxLength = attribute.MaxLength;
                attributeEntity.MinLength = attribute.MinLength;
                attributeEntity.Max = attribute.Max;
                attributeEntity.Min = attribute.Min;
                attributeEntity.ListId = attribute.ListId;
                attributeEntity.GroupId = attribute.GroupId;
                attributeEntity.MaxDate = attribute.MaxDate;
                attributeEntity.MinDate = attribute.MinDate;
                attributeEntity.Template = attribute.Template;

                _logger.Log(LogLevel.Trace, $"Removing attribute categories...");
                foreach (var attributeCategory in attributeEntity.AttributeCategories)
                {
                    if (attribute.AttributeCategories.All(ac => ac.CategoryId != attributeCategory.CategoryId))
                    {
                        _context.AttributeCategories.Remove(attributeCategory);
                    }
                }

                _logger.Log(LogLevel.Trace, "Adding attribute categories...");
                foreach (var newCategoryId in attribute.AttributeCategories.Select(ac => ac.CategoryId))
                {
                    if (attributeEntity.AttributeCategories.All(ac => ac.CategoryId != newCategoryId))
                    {
                        _context.AttributeCategories.Add(new AttributeCategory() { CategoryId = newCategoryId, AttributeId = attributeEntity.Id });
                    }
                }

                _logger.Log(LogLevel.Trace, "Removing  attribute permissions...");
                attributeEntity.AttributePermissions.RemoveAll(ap => !attribute.AttributePermissions.Select(p => p.Id).Contains(ap.Id));

                _logger.Log(LogLevel.Trace, "Adding attribute permissions...");
                foreach (var permission in attribute.AttributePermissions)
                {
                    var permissionEntity = await _context.AttributePermissions.FirstOrDefaultAsync(p => p.Id == permission.Id);

                    if (permissionEntity == null)
                    {
                        _context.AttributePermissions.Add(new AttributePermission { Value = permission.Value, AttributeId = attributeEntity.Id, RoleId = permission.RoleId });
                    }
                    else
                    {
                        permissionEntity.Value = permission.Value;
                        permissionEntity.RoleId = permission.RoleId;
                    }
                }             

                _logger.Log(LogLevel.Trace, $"Saving '{attributeEntity.Name}' attribute to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{attributeEntity.Name}' attribute Error: {e.Message}");
                    return BadRequest($"Error on saving '{attributeEntity.Name}' attribute");
                }
                _logger.Log(LogLevel.Trace, $"Attribute '{attributeEntity.Name}' was successfully saved");

                _logger.Log(LogLevel.Debug, "Editing attribute is finished.");
                return Ok(new { Attribute = _transformModelHelper.TransformAttribute(attribute, true), WithCategories = true });
            }
            else
            {
                _logger.Log(LogLevel.Info, $"Attribute with same 'Name': '{attribute.Name}' already exist.");
                return BadRequest($"Attribute with same 'Name': '{attribute.Name}' already exist.");
            }
        }

        [HttpPut("groups/{id}")]
        public async Task<IActionResult> EditGroup([FromRoute] int id, [FromBody] AttributeGroup attributeGroup)
        {
            _logger.Log(LogLevel.Debug, $"Start editing attribute group with id='{id}'...");
            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }            

            _logger.Log(LogLevel.Trace, "Getting attribute group...");
            var attributeGroupEntity = await _context.AttributeGroups.FindAsync(id);
            
            if (attributeGroupEntity == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute group with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute '{attributeGroupEntity.Name}' group successfully received");

            if (!_context.AttributeGroups.Any(ag => id != ag.Id && ag.Name == attributeGroup.Name && ag.DeleteTime == null))
            {
                _logger.Log(LogLevel.Trace, "Updating attribute grop...");
                attributeGroupEntity.Name = attributeGroup.Name;
              
                _logger.Log(LogLevel.Trace, $"Saving '{attributeGroupEntity.Name}' attribute group to database...");
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, $"Error on saving '{attributeGroupEntity.Name}' attribute group Error: {e.Message}");
                    return BadRequest($"Error on saving '{attributeGroupEntity.Name}' attribute group");
                }
                _logger.Log(LogLevel.Trace, $"Attribute group '{attributeGroupEntity.Name}' was successfully saved");

                _logger.Log(LogLevel.Debug, "Editing attribute group is finished.");
                return Ok(attributeGroupEntity);
            }
            else
            {
                _logger.Log(LogLevel.Info, $"AttributeGroup with same 'Name': '{attributeGroup.Name}' already exist.");
                return BadRequest($"AttributeGroup with same 'Name': '{attributeGroup.Name}' already exist.");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {            
            _logger.Log(LogLevel.Debug, $"Start deleting attribute with is='{id}'...");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
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

            _logger.Log(LogLevel.Trace, "Getting attribute...");
            var attribute = await _context.Attributes.Include(a => a.AttributeCategories).Include(a => a.AttributePermissions).FirstOrDefaultAsync(a => a.Id == id);

            if (attribute == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute group '{attribute.Name}' successfully received");

            _logger.Log(LogLevel.Trace, "Checking permissions...");
            if (!attribute.AttributePermissions.Any(p => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id).Contains(p.RoleId) && p.Value > DataAccessMethods.Read))
            {
                return Forbid();
            }
            _logger.Log(LogLevel.Trace, "Permissions are checked...");

            _logger.Log(LogLevel.Trace, $"Updating '{attribute.Name}' attribute fields...");
            attribute.DeleterId = user.Id;
            attribute.DeleteTime = DateTime.Now;

            _context.AttributePermissions.RemoveRange(attribute.AttributePermissions);           

            _logger.Log(LogLevel.Trace, $"Saving '{attribute.Name}' attribute to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving '{attribute.Name}' attribute  Error: {e.Message}");
                return BadRequest($"Error on saving '{attribute.Name}' attribute");
            }
            _logger.Log(LogLevel.Trace, $"Attribute '{attribute.Name}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting attribute is finished.");
            return Ok(new { Attribute = _transformModelHelper.TransformAttribute(attribute, true), WithCategories = true });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAttributes()
        {          
            _logger.Log(LogLevel.Debug, "Start deleting attributes...");

            _logger.Log(LogLevel.Trace, "Getting ids of attributes...");
            var ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToArray();
            _logger.Log(LogLevel.Trace, $"Ids('{String.Join(',', ids)}') of attributes successfully received");         

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));
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

            var removedAttributes = new List<Attribute>();
            _logger.Log(LogLevel.Trace, "Updating attributes fields...");
            foreach (var id in ids)
            {
                var attribute = await _context.Attributes.Include(a => a.AttributeCategories).Include(a => a.AttributePermissions).FirstOrDefaultAsync(a => a.Id == id);

                if (attribute == null)
                {
                    return NotFound();
                }

                if (!attribute.AttributePermissions.Any(p => user.UserRoles.Select(ur => ur.Role).Select(r => r.Id).Contains(p.RoleId) && p.Value > DataAccessMethods.Read))
                {
                    return Forbid();
                }

                attribute.DeleterId = user.Id;
                attribute.DeleteTime = DateTime.Now;

                _context.AttributeCategories.RemoveRange(_context.AttributeCategories.Where(ac => ac.AttributeId == attribute.Id));

                _context.AttributePermissions.RemoveRange(attribute.AttributePermissions);

                removedAttributes.Add(attribute);
            }
            _logger.Log(LogLevel.Trace, "Updating attributes fields is finished");
           
            _logger.Log(LogLevel.Trace, "Saving attributes to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving attributes. Error: {e.Message}");
                return BadRequest("Error on saving attributes");
            }
            _logger.Log(LogLevel.Trace, "Attributes was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting attributes is finished.");
            return Ok(removedAttributes.Select(a => new { Attribute = _transformModelHelper.TransformAttribute(a, true), WithCategories = true }));
        }

        [HttpDelete("groups/{id}")]
        public async Task<IActionResult> DeleteGroup([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Start deleting attribute group with id='{id}'...");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Getting attribute group...");
            var attributeGroup = await _context.AttributeGroups.FindAsync(id);

            if (attributeGroup == null)
            {
                _logger.Log(LogLevel.Error, $"Attribute group with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"Attribute group '{attributeGroup.Name}' successfully received");

            _logger.Log(LogLevel.Trace, $"Updating '{attributeGroup.Name}' attribute group fields...");
            attributeGroup.DeleterId = userId;
            attributeGroup.DeleteTime = DateTime.Now;

            _logger.Log(LogLevel.Trace, "Getting attribute...");
            var attributes = await _context.Attributes.Where(a => a.GroupId == attributeGroup.Id).ToListAsync();
            _logger.Log(LogLevel.Trace, "Attributes successfully received");

            _logger.Log(LogLevel.Trace, "Updating attribute fields...");
            foreach (var attribute in attributes)
            {
                attribute.GroupId = null;
            }
            _logger.Log(LogLevel.Trace, "Updating attribute fields is finished");
            _logger.Log(LogLevel.Trace, $"Updating '{attributeGroup.Name}' attribute group fields is finished");

            _logger.Log(LogLevel.Trace, $"Saving '{attributeGroup.Name}' attribute group to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving '{attributeGroup.Name}' attribute group Error: {e.Message}");
                return BadRequest($"Error on saving '{attributeGroup.Name}' attribute group");
            }
            _logger.Log(LogLevel.Trace, $"Attribute group '{attributeGroup.Name}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting attribute group is finished.");
            return Ok(attributeGroup);
        }

        [HttpDelete("groups")]
        public async Task<IActionResult> DeleteGroups()
        {
            _logger.Log(LogLevel.Debug, "Start deleting attribute groups...");
          
            _logger.Log(LogLevel.Trace, "Getting ids of attribute groups...");
            var ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToArray();
            _logger.Log(LogLevel.Trace, $"Ids('{String.Join(',', ids)}') of attribute groups successfully received");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            var removedAttributeGroups = new List<AttributeGroup>();

            _logger.Log(LogLevel.Trace, "Getting attribute groups...");
            var attributeGroups = await _context.AttributeGroups.Include(ag => ag.Attributes).Where(ag => ids.Contains(ag.Id)).ToListAsync();
            _logger.Log(LogLevel.Trace, "Attribute groups successfully received");

            _logger.Log(LogLevel.Trace, "Updating attribute groups...");
            foreach (var attributeGroup in attributeGroups)
            {
                if (attributeGroup == null)
                {
                    return NotFound();
                }

                attributeGroup.DeleterId = userId;
                attributeGroup.DeleteTime = DateTime.Now;
                removedAttributeGroups.Add(attributeGroup);

                foreach (var attribute in attributeGroup.Attributes)
                {
                    attribute.GroupId = null;
                }
            }
            _logger.Log(LogLevel.Trace, "Updating attribute groups is finished");
           
            _logger.Log(LogLevel.Trace, "Saving attribute groups to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving attribute groups Error: {e.Message}");
                return BadRequest("Error on saving attribute groups");
            }
            _logger.Log(LogLevel.Trace, "Attribute groups was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting attribute groups is finished.");
            return Ok(removedAttributeGroups);
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilteredAttributes([FromQuery] int categoryId)
        {
            _logger.Log(LogLevel.Trace, $"Getting filtered attributes by category id #{categoryId}...");

            if (!_context.Categories.Any(c => c.Id == categoryId))
            {
                _logger.Log(LogLevel.Error, $"Category with id #{categoryId} was not found.");
                return BadRequest($"Category with id #{categoryId} was not found.");
            }

            _logger.Log(LogLevel.Trace, $"Returning filtered attributes by category id #{categoryId}...");
            return Ok(await _context.Attributes
                .Include(a => a.AttributeCategories)
                .Where(a => a.AttributeCategories.Any(ac => ac.CategoryId == categoryId && ac.IsFiltered == true))
                .Select(a => _transformModelHelper.TransformAttribute(a, false, false))
                .ToArrayAsync());
        }

        private async Task ChangeAttributeType(Attribute attributeEntity, Attribute attribute)
        {
            Action<AttributeValue> converter = null;

            switch (attributeEntity.Type)
            {
                case AttributeType.Boolean:
                    {
                        if (attribute.Type == AttributeType.String)
                            converter = (av) =>
                            {
                                if (av.BoolValue.HasValue)
                                {
                                    av.StrValue = av.BoolValue.Value ? "Да" : "Нет";
                                    av.BoolValue = null;
                                }
                            };
                    }
                    break;

                case AttributeType.String:
                    {
                        if (attribute.Type == AttributeType.Number)
                            converter = (av) =>
                            {
                                if (double.TryParse(av.StrValue, out var numValue))
                                {
                                    av.NumValue = numValue;
                                    av.StrValue = null;
                                }
                            };

                        if (attribute.Type == AttributeType.List)
                            converter = (av) =>
                            {
                                var listValue = _context.ListValues.Local.FirstOrDefault(l => l.ListId == attributeEntity.ListId && l.Value?.ToUpper().Trim() == av.StrValue?.ToUpper().Trim());
                                if (listValue != null)
                                {
                                    av.ListValueId = listValue.Id;
                                    av.StrValue = null;
                                }
                            };



                    }
                    break;

                case AttributeType.Number:
                    {
                        if (attribute.Type == AttributeType.String)
                            converter = (av) =>
                            {
                                av.StrValue = av.NumValue?.ToString();
                                av.NumValue = null;
                            };

                        if (attribute.Type == AttributeType.List)
                            converter = (av) =>
                            {
                                if (av.NumValue.HasValue)
                                {
                                    var listValue = _context.ListValues.Local.FirstOrDefault(l => l.ListId == attributeEntity.ListId && l.Value == av.NumValue.ToString());
                                    if (listValue != null)
                                    {
                                        av.ListValueId = listValue.Id;
                                        av.NumValue = null;
                                    }
                                }
                            };

                    }
                    break;

                case AttributeType.List:
                    {
                        if (attribute.Type == AttributeType.String)
                            converter = (av) =>
                            {
                                if (av.ListValue != null)
                                {
                                    av.StrValue = av.ListValue.Value;
                                    av.ListValueId = null;
                                    av.ListValue = null;
                                }
                            };


                        if (attribute.Type == AttributeType.Number)
                            converter = (av) =>
                            {
                                if (double.TryParse(av.ListValue?.Value, out var numValue))
                                {
                                    av.NumValue = numValue;
                                    av.ListValueId = null;
                                    av.ListValue = null;
                                }
                            };
                    }
                    break;
                case AttributeType.Date:
                    {

                        if (attribute.Type == AttributeType.String)
                            converter = (av) =>
                            {
                                av.StrValue = av.DateValue?.ToString();
                                av.DateValue = null;
                            };
                    }
                    break;

            }

            if (converter != null)
            {
                await _context.AttributeValues
                    .Include(av => av.ListValue)
                    .Where(av => av.AttributeId == attributeEntity.Id).LoadAsync();

                if (attribute.Type == AttributeType.List)
                {
                    await _context.ListValues.Where(l => l.ListId == attribute.ListId).LoadAsync();
                }

                attributeEntity.AttributeValues.ForEach(converter);
            }
        }
    }
}