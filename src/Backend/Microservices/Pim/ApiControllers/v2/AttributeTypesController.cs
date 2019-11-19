using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Company.Common.Extensions;
using Company.Common.Models.Pim;
using Company.Pim.Helpers.v2;
using NLog;

namespace Company.Pim.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/attributes/types")]
    public class AttributeTypesController : Controller
    {
        private readonly PimContext _context;
        private TransformModelHelpers _transformModelHelper { get; set; }
        private readonly Logger _logger;

        public AttributeTypesController(PimContext context, TransformModelHelpers transformModelHelpers)
        {
            _context = context;
            _transformModelHelper = transformModelHelpers;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("lists")]
        public async Task<IActionResult> GetLists()
        {

            _logger.Log(LogLevel.Debug, "Start getting lists...");

            var lists = await _context.Lists.Where(l => l.DeleteTime == null)
                .Include(l => l.ListValues)
                .Include(l => l.ListMetadatas)
                .ThenInclude(lm => lm.ListValueMetadatas)
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "Lists successfully received");

            _logger.Log(LogLevel.Trace, "Filling list values and list metadatas...");
            foreach (var list in lists)
            {
                list.ListValues = list.ListValues.Where(lv => lv.DeleteTime == null).ToList();
                list.ListMetadatas = list.ListMetadatas.Where(lv => lv.DeleteTime == null).ToList();

                foreach (var listMetadata in list.ListMetadatas)
                {
                    listMetadata.ListValueMetadatas = listMetadata.ListValueMetadatas.Where(lvm => lvm.DeleteTime == null).ToList();
                }

                foreach (var listValue in list.ListValues)
                {
                    listValue.ListValueMetadatas = listValue.ListValueMetadatas.Where(lvm => lvm.DeleteTime == null).ToList();
                }
            }
            _logger.Log(LogLevel.Trace, "List values and list metadatas are filled");

            _logger.Log(LogLevel.Debug, "Getting lists is finished.");
            return Ok(lists.OrderBy(l => l.Name).Select(l => _transformModelHelper.TransformList(l)).ToList());
        }

        [HttpGet("lists/{id}")]
        public async Task<IActionResult> GetList([FromRoute] int id)
        {

            _logger.Log(LogLevel.Trace, $"Getting list by id='{id}'...");
            var list = await _context.Lists.Include(l => l.ListValues)
                .Include(l => l.ListMetadatas)
                .ThenInclude(lm => lm.ListValueMetadatas)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                _logger.Log(LogLevel.Error, $"List with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"List '{list.Name}' successfully received");

            _logger.Log(LogLevel.Trace, $"Getting '{list.Name}' list values ...");
            list.ListValues = list.ListValues.Where(lv => lv.DeleteTime == null).OrderBy(v => v.Value).ToList();
            _logger.Log(LogLevel.Trace, "List values successfully received");

            _logger.Log(LogLevel.Trace, $"Getting '{list.Name}' list metadatas...");
            list.ListMetadatas = list.ListMetadatas.Where(lv => lv.DeleteTime == null).OrderBy(v => v.Name).ToList();
            _logger.Log(LogLevel.Trace, $"List '{list.Name}' metadatas successfully received");

            _logger.Log(LogLevel.Trace, $"Filling list value metadatas of '{list.Name}' list...");
            foreach (var listMetadata in list.ListMetadatas)
            {
                listMetadata.ListValueMetadatas = listMetadata.ListValueMetadatas.Where(lvm => lvm.DeleteTime == null).ToList();
            }

            foreach (var listValue in list.ListValues)
            {
                listValue.ListValueMetadatas = listValue.ListValueMetadatas.Where(lvm => lvm.DeleteTime == null).ToList();
            }
            _logger.Log(LogLevel.Trace, $"List value metadatas of '{list.Name}' list are filled");

            _logger.Log(LogLevel.Debug, "Getting list is finished.");
            return Ok(_transformModelHelper.TransformList(list));
        }


        [HttpGet("list-values/{id}")]
        public async Task<IActionResult> GetListValue([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Start getting list value with id='{id}'...");

            var listValue = await _context.ListValues
                .Include(l => l.ListValueMetadatas)
                .ThenInclude(lm => lm.ListMetadata)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listValue == null)
            {
                _logger.Log(LogLevel.Error, $"List value  with id='{id}' not found");
                return NotFound();
            }

            _logger.Log(LogLevel.Trace, $"List value of listId('{listValue.ListId}') successfully received");

            _logger.Log(LogLevel.Debug, "Getting list value is finished.");
            return Ok(_transformModelHelper.TransformListValue(listValue));
        }

        [HttpGet("listValueCmpyIds")]
        public async Task<IActionResult> GetListValueCmpyIds()
        {
            _logger.Log(LogLevel.Debug, "Start getting list values by ids...");
            var ids = new List<int>();

            _logger.Log(LogLevel.Trace, "Getting ids from request query...");
            if (Request.Query.ContainsKey("ids") && !string.IsNullOrEmpty(Request.Query["ids"].ToString()))
            {
                ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, $"Ids('{String.Join(',', ids)}') successfully received");
            }

            _logger.Log(LogLevel.Trace, "Getting list values by ids...");
            var listValues = await _context.ListValues
                .Include(l => l.ListValueMetadatas)
                .ThenInclude(lm => lm.ListMetadata)
                .Where(l => ids.Contains(l.Id))
                .ToListAsync();

            if (!listValues.Any())
            {
                _logger.Log(LogLevel.Error, $"List values with ids='{String.Join(',', ids)}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, "List values successfully received");

            _logger.Log(LogLevel.Debug, "Getting list values by ids is finished.");
            return Ok(listValues.Select(lv => _transformModelHelper.TransformListValue(lv)));
        }

        [HttpGet("listCmpyIds")]
        public async Task<IActionResult> GetListCmpyIds()
        {
            _logger.Log(LogLevel.Debug, "Start getting lists by ids...");
            var ids = new List<int>();

            _logger.Log(LogLevel.Trace, "Getting ids from request query...");
            if (Request.Query.ContainsKey("ids") && !string.IsNullOrEmpty(Request.Query["ids"].ToString()))
            {
                ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToList();
                _logger.Log(LogLevel.Trace, "Ids successfully received");
            }

            _logger.Log(LogLevel.Trace, "Getting lists by ids...");
            var lists = await _context.Lists.Where(l => l.DeleteTime == null)
                .Include(l => l.ListValues)
                .Where(l => ids.Contains(l.Id))
                .OrderBy(l => l.Name)
                .ToListAsync();
            _logger.Log(LogLevel.Trace, "List successfully received");

            _logger.Log(LogLevel.Trace, "Filling lists values...");
            foreach (var list in lists)
            {
                list.ListValues = list.ListValues.Where(lv => lv.DeleteTime == null).OrderBy(lv => lv.Value).ToList();
            }
            _logger.Log(LogLevel.Trace, "Lists values are filled");

            _logger.Log(LogLevel.Debug, "Get lists by ids is finished.");
            return Ok(lists.Select(l => _transformModelHelper.TransformList(l, false, true)));
        }


        [HttpPost("lists")]
        public async Task<IActionResult> CreateList([FromBody] List list)
        {

            _logger.Log(LogLevel.Debug, $"Start creating list with name='{list.Name}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, $"Checking '{list.Name}' list metadatas...");
            if (list.ListMetadatas.GroupBy(lm => lm.Name).Any(g => g.Count() > 1))
            {
                _logger.Log(LogLevel.Error, "The same value names are not allowed.");
                return BadRequest("Недопустимы одинаковые названия значений.");
            }
            _logger.Log(LogLevel.Trace, $"List metadatas of '{list.Name}' list are checked");

            _logger.Log(LogLevel.Trace, "Creating new list...");
            var l = new List()
            {
                Name = list.Name,
                Template = list.Template,
                CreateTime = DateTime.Now,
                CreatorId = userId,
                ListMetadatas = list.ListMetadatas.Select(lm => new ListMetadata() { Name = lm.Name }).ToList(),
                ListValues = list.ListValues.Select(lv => new ListValue() { Value = lv.Value }).ToList()
            };
            _logger.Log(LogLevel.Trace, $"List '{l.Name}' is created...");

            _logger.Log(LogLevel.Trace, "Filling list value and list metadata...");
            foreach (var listValue in list.ListValues)
            {
                listValue.ListValueMetadatas.ForEach(lmd => _context.ListValueMetadatas.Add(new ListValueMetadata()
                {
                    Value = lmd.Value,
                    ListValue = l.ListValues.First(lv => lv.Value == listValue.Value),
                    ListMetadata = l.ListMetadatas.First(lm => lm.Name == lmd.ListMetadata.Name)
                }));
            }
            _logger.Log(LogLevel.Trace, "List value and list metadata are filled");

            _context.Lists.Add(l);

            _logger.Log(LogLevel.Trace, $"Saving '{l.Name}' list to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving '{l.Name}' list. Error: {e.Message}");
                return BadRequest($"Error on save '{l.Name}' list");
            }
            _logger.Log(LogLevel.Trace, $"List '{l.Name}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Creating list is finished.");
            return Ok(_transformModelHelper.TransformList(l));
        }

        [HttpPost("listvalues")]
        public async Task<IActionResult> CreateListValue([FromBody] ListValue listValue)
        {

            _logger.Log(LogLevel.Debug, $"Start creating list value='{listValue.Value}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            _context.ListValues.Add(listValue);

            _logger.Log(LogLevel.Trace, "Saving list value to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving list value. Error: {e.Message}");
                return BadRequest("Error on save list value");
            }
            _logger.Log(LogLevel.Trace, $"List value with ID: '{listValue.Id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Creating list value is finished.");
            return Ok(_transformModelHelper.TransformListValue(listValue));
        }

        [HttpPut("lists/{id}")]
        public async Task<IActionResult> EditList([FromRoute] int id, [FromBody] List list)
        {
            _logger.Log(LogLevel.Debug, $"Start editing list wiht id='{id}'...");

            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, "Checking list metadatas...");
            if (list.ListMetadatas.GroupBy(lm => lm.Name).Any(g => g.Count() > 1))
            {
                _logger.Log(LogLevel.Error, "The same value names are not allowed.");
                return BadRequest("Недопустимы одинаковые названия значений.");
            }
            _logger.Log(LogLevel.Trace, "List metadatas are checked");

            _logger.Log(LogLevel.Trace, "Getting list entity...");
            var listEntity = await _context.Lists.Include(l => l.ListValues)
                .Include(l => l.ListMetadatas)
                .ThenInclude(lm => lm.ListValueMetadatas)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (listEntity == null)
            {
                _logger.Log(LogLevel.Error, $"List entity  with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"List entity '{listEntity.Name}' successfully received");

            _logger.Log(LogLevel.Trace, $"Filling list entity datas of '{listEntity.Name}' list entity...");
            listEntity.Name = list.Name;
            listEntity.Template = list.Template;

            listEntity.ListValues.Where(lv => !list.ListValues.Select(lvd => lvd.Id).Contains(lv.Id))
                .ToList()
                .ForEach(lv =>
                {
                    lv.DeleteTime = DateTime.Now;
                    lv.DeleterId = userId;
                    lv.ListValueMetadatas.ForEach(lvm =>
                    {
                        lvm.DeleteTime = DateTime.Now;
                        lvm.DeleterId = userId;
                    });
                });
            _logger.Log(LogLevel.Trace, $"List entity datas of '{listEntity.Name}' list entity are filled");

            _logger.Log(LogLevel.Trace, $"Filling list values of the '{listEntity.Name}' list entity...");
            listEntity.ListValues.Where(lv => list.ListValues.Select(lvd => lvd.Id).Contains(lv.Id))
                .ToList()
                .ForEach(lv =>
                {
                    lv.ListValueMetadatas.ForEach(lvm =>
                    {
                        lvm.DeleteTime = DateTime.Now;
                        lvm.DeleterId = userId;
                    });
                });
            _logger.Log(LogLevel.Trace, $"List values of the '{listEntity.Name}' list entity are filled");

            _logger.Log(LogLevel.Trace, $"Filling list metadatas of the '{listEntity.Name}' list entity...");
            listEntity.ListMetadatas.Where(
                    lm => !list.ListMetadatas.Select(lmd => lmd.Id)
                        .Contains(lm.Id))
                .ToList()
                .ForEach(
                    lm =>
                    {
                        lm.DeleteTime = DateTime.Now;
                        lm.DeleterId = userId;
                        lm.ListValueMetadatas.ForEach(lvm =>
                        {
                            lvm.DeleteTime = DateTime.Now;
                            lvm.DeleterId = userId;
                        });
                    }
                );
            _logger.Log(LogLevel.Trace, $"List metadatas of the '{listEntity.Name}' list entity are filled");

            _logger.Log(LogLevel.Trace, "Updating attribute values...");
            _context.AttributeValues.Where(av => av.ListValueId != null && listEntity.ListValues.Where(lv => lv.DeleteTime != null)
                                                     .Select(lv => lv.Id)
                                                     .Contains(av.ListValueId.Value))
                .ToList()
                .ForEach(av => av.ListValueId = null);
            _logger.Log(LogLevel.Trace, "Attribute values are updated");

            _logger.Log(LogLevel.Trace, "Updating list values...");
            foreach (var listValue in list.ListValues)
            {
                var listValueEntity = await _context.ListValues.FindAsync(listValue.Id);

                if (listValueEntity == null)
                {
                    _context.ListValues.Add(new ListValue { Value = listValue.Value, ListId = listEntity.Id });
                }
                else
                {
                    listValueEntity.Value = listValue.Value;
                }
            }
            _logger.Log(LogLevel.Trace, "List values are updated");

            _logger.Log(LogLevel.Trace, "Updating list metadatas...");
            foreach (var listMetadata in list.ListMetadatas)
            {
                var listMetadataEntity = await _context.ListMetadatas.FindAsync(listMetadata.Id);

                if (listMetadataEntity == null)
                {
                    _context.ListMetadatas.Add(new ListMetadata { Name = listMetadata.Name, ListId = listEntity.Id });
                }
                else
                {
                    listMetadataEntity.Name = listMetadata.Name;
                }
            }
            _logger.Log(LogLevel.Trace, "List metadatas are updated");

            _logger.Log(LogLevel.Trace, "Filling list values and list metadatas");
            foreach (var listValue in list.ListValues)
            {
                foreach (var listValueMetadata in listValue.ListValueMetadatas)
                {
                    var listValueMetadataEntity = await _context.ListValueMetadatas.FirstOrDefaultAsync(lvm => lvm.ListMetadataId == listValueMetadata.Id && lvm.ListValueId == listValue.Id);

                    if (listValueMetadataEntity == null)
                    {
                        _context.ListValueMetadatas.Add(new ListValueMetadata()
                        {
                            Value = listValueMetadata.Value,
                            ListValue = listEntity.ListValues.First(lv => lv.DeleteTime == null && lv.Value == listValue.Value),
                            ListMetadata = listEntity.ListMetadatas.First(lm => lm.DeleteTime == null && lm.Name == listValueMetadata.ListMetadata.Name)
                        });
                    }
                    else
                    {
                        listValueMetadataEntity.Value = listValueMetadata.Value;
                    }
                }
            }
            _logger.Log(LogLevel.Trace, "List values and list metadatas are filled");

            _logger.Log(LogLevel.Trace, $"Saving '{listEntity.Name}' list to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving '{listEntity.Name}' list. Error: {e.Message}");
                return BadRequest($"Error on save '{listEntity.Name}' list");
            }
            _logger.Log(LogLevel.Trace, $"List '{listEntity.Name}' was successfully saved");

            listEntity.ListValues = listEntity.ListValues.Where(lv => lv.DeleteTime == null).ToList();
            listEntity.ListMetadatas = listEntity.ListMetadatas.Where(lm => lm.DeleteTime == null).ToList();

            _logger.Log(LogLevel.Debug, "Editing list is finished.");
            return Ok(_transformModelHelper.TransformList(list));

        }

        [HttpPut("listvalues/{id}")]
        public async Task<IActionResult> EditListValue([FromRoute] int id, [FromBody] ListValue listvalue)
        {
            _logger.Log(LogLevel.Debug, $"Start editing list value with id='{id}'...");
            if (!ModelState.IsValid)
            {
                _logger.Log(LogLevel.Warn, "Model is not valid");
                return BadRequest(ModelState);
            }

            _logger.Log(LogLevel.Trace, "Getting list value entity...");
            var listValueEntity = await _context.ListValues.FirstOrDefaultAsync(lv => lv.Id == id);

            if (listValueEntity == null)
            {
                _logger.Log(LogLevel.Error, $"List value entity with id='{id}' not found");
                return NotFound();
            }

            _logger.Log(LogLevel.Trace, $"List value entity with ID: '{listValueEntity.Id}' successfully received");

            _logger.Log(LogLevel.Trace, $"Updating list value entity with ID: '{listValueEntity.Id}'...");
            listValueEntity.Value = listvalue.Value;
            listValueEntity.ListId = listvalue.ListId;
            _logger.Log(LogLevel.Trace, $"List value entity with ID: '{listValueEntity.Id}' is updated");

            _logger.Log(LogLevel.Trace, $"Saving list value entity with ID: '{listValueEntity.Id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving list value entity with ID: '{listValueEntity.Id}'. Error: {e.Message}");
                return BadRequest($"Error on save list value entity with ID: '{listValueEntity.Id}'");
            }
            _logger.Log(LogLevel.Trace, $"List value entity with ID: '{listValueEntity.Id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Editing list value is finished.");
            return Ok(_transformModelHelper.TransformListValue(listvalue));
        }

        [HttpDelete("lists/{id}")]
        public async Task<IActionResult> DeleteList([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Start deleting list with id='{id}'...");

            _logger.Log(LogLevel.Trace, $"Getting list by id='{id}'...");
            var list = await _context.Lists
                .Include(l => l.ListValues)
                .Include(l => l.ListMetadatas)
                .ThenInclude(lm => lm.ListValueMetadatas)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (list == null)
            {
                _logger.Log(LogLevel.Error, $"List with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"List '{list.Name}' successfully received");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, $"Getting attributes of the '{list.Name}' list...");
            var attributes = _context.Attributes.Where(a => a.DeleteTime == null && a.ListId == list.Id).ToList();
            _logger.Log(LogLevel.Trace, $"Attributes of the '{list.Name}' list successfully received");

            if (attributes.Any(a => a.Type == AttributeType.List))
            {
                _logger.Log(LogLevel.Error, $"Couldn't delete list, it have some references to another attributes:  { string.Join(",", Array.ConvertAll(attributes.Where(a => a.Type == AttributeType.List).Select(a => a.Name).ToArray(), name => "'" + name + "'"))}");
                return BadRequest($"Невозможно выполнить удаление т.к. к данному списку привязаны следующие атрибуты: " +
                                  $"{string.Join(",", Array.ConvertAll(attributes.Where(a => a.Type == AttributeType.List).Select(a => a.Name).ToArray(), name => "'" + name + "'"))}");
            }
            else
            {
                _logger.Log(LogLevel.Trace, "Updating attributes...");
                attributes.ForEach(a => a.ListId = null);
                _logger.Log(LogLevel.Trace, "Attributes are updated");
            }

            _logger.Log(LogLevel.Trace, $"Updating '{list.Name}' list...");
            list.DeleterId = userId;
            list.DeleteTime = DateTime.Now;
            _logger.Log(LogLevel.Trace, $"'{list.Name}' list is updated");

            _logger.Log(LogLevel.Trace, "Updating lists values...");
            foreach (var listValue in list.ListValues)
            {
                listValue.DeleterId = userId;
                listValue.DeleteTime = DateTime.Now;
            }
            _logger.Log(LogLevel.Trace, "Lists values are updated");

            _logger.Log(LogLevel.Trace, $"Updating '{list.Name}' list metadatas...");
            list.ListMetadatas.ForEach(
                lm =>
                {
                    lm.DeleteTime = DateTime.Now;
                    lm.DeleterId = userId;
                    lm.ListValueMetadatas.ForEach(lvm =>
                    {
                        lvm.DeleteTime = DateTime.Now;
                        lvm.DeleterId = userId;
                    });
                }
            );
            _logger.Log(LogLevel.Trace, $"'{list.Name}' list metadatas are updated");

            _logger.Log(LogLevel.Trace, $"Saving '{list.Name}' list to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving '{list.Name}' list. Error: {e.Message}");
                return BadRequest($"Error on save '{list.Name}' list");
            }
            _logger.Log(LogLevel.Trace, $"'{list.Name}' list was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting list is finished.");
            return Ok(_transformModelHelper.TransformList(list));
        }

        [HttpDelete("listvalues/{id}")]
        public async Task<IActionResult> DeleteListValue([FromRoute] int id)
        {
            _logger.Log(LogLevel.Debug, $"Start deleting list value with id='{id}'...");

            _logger.Log(LogLevel.Trace, $"Getting list value by id='{id}' ...");
            var listValue = await _context.ListValues.FirstOrDefaultAsync(lv => lv.Id == id);

            if (listValue == null)
            {
                _logger.Log(LogLevel.Error, $"List value with id='{id}' not found");
                return NotFound();
            }
            _logger.Log(LogLevel.Trace, $"List value with ID='{listValue.Id}' successfully received");

            var userId = Convert.ToInt32(Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId"));

            _logger.Log(LogLevel.Trace, $"Updating list value with ID='{listValue.Id}'...");
            listValue.DeleterId = userId;
            listValue.DeleteTime = DateTime.Now;
            _logger.Log(LogLevel.Trace, $"List value with ID='{listValue.Id}' are updated");

            _logger.Log(LogLevel.Trace, $"Saving list value with ID='{listValue.Id}' to database...");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.Log(LogLevel.Error, $"Error on saving list value with ID='{listValue.Id}'. Error: {e.Message}");
                return BadRequest($"Error on save list value with ID='{listValue.Id}'");
            }
            _logger.Log(LogLevel.Trace, $"List value with ID='{listValue.Id}' was successfully saved");

            _logger.Log(LogLevel.Debug, "Deleting list value is finished.");
            return Ok(_transformModelHelper.TransformListValue(listValue));
        }
    }
}