using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;

namespace Company.FileStorage.ApiControllers.v2
{
    [Produces("application/json")]
    [Route("v2/[controller]")]
    public class FilesController : Controller
    {
        private readonly FileStorageContext _context;
        private readonly FileStorageManager _manager;
        private readonly Logger _logger;

        public FilesController(FileStorageContext context, FileStorageManager manager)
        {
            _context = context;
            _manager = manager;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpPost]
        public async Task<IActionResult> Save()
        {
            _logger.Log(LogLevel.Debug, "Starting the save file method...");

            _logger.Log(LogLevel.Trace, "Getting file from request...");
            var reqFile = HttpContext.Request.Form.Files.FirstOrDefault();

            if (reqFile != null)
            {
                _logger.Log(LogLevel.Trace, $"File {reqFile.Name} from request has been got.");
                var userId = Common.Helpers.Headers.GetHeaderValue(Request.Headers, "UserId");

                _logger.Log(LogLevel.Trace, $"Saving {reqFile.Name} file...");
                var res = _manager.SaveFile(_context, reqFile, Convert.ToInt32(userId));
                _logger.Log(LogLevel.Trace, $"File {reqFile.Name} was successfully saved");

                _logger.Log(LogLevel.Debug, "Save file method is over.");
                return Ok(res);
            }
            else
            {
                _logger.Log(LogLevel.Error, "File not found in request.");
                return BadRequest("File not found in request.");
            }
        }

        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            _logger.Log(LogLevel.Debug, $"Starting the get file with id='{id}' method...");

            _logger.Log(LogLevel.Trace, "Getting file...");
            var file = _manager.Get(_context, id);
            if (file != null && System.IO.File.Exists(Path.Combine(_manager.BasePath, file.Path)))
            {
                var fileStreamResult = new FileStreamResult(new FileStream(Path.Combine(_manager.BasePath, file.Path),
                                                            FileMode.Open,
                                                            FileAccess.Read,
                                                            FileShare.Read | FileShare.Delete), "application/octet-stream");

                _logger.Log(LogLevel.Trace, $"File with ID='{id}' and Name:'{file.Name}' successfully fetched.");
                fileStreamResult.FileDownloadName = file.Name;

                _logger.Log(LogLevel.Debug, "Get file method is over.");
                return fileStreamResult;
            }
            else
            {
                _logger.Log(LogLevel.Error, $"File with id='{id}' not found in file storage");
                return NotFound($"File with id='{id}' not found in file storage");
            }
        }

        [HttpGet("metadata")]
        public ActionResult GetMetadata()
        {
            _logger.Log(LogLevel.Debug, "Starting the get metadata method...");

            _logger.Log(LogLevel.Trace, "Getting ids from the request query...");
            var ids = Request.Query["ids"].ToString().Split(',').Select(int.Parse).ToArray();
            _logger.Log(LogLevel.Trace, "Ids values successfully fetched");

            var files = new List<Common.Models.FileStorage.File>();

            _logger.Log(LogLevel.Trace, "Getting metadatas...");
            foreach (var id in ids)
            {
                var file = _manager.Get(_context, id);
                if (file != null)
                {
                    files.Add(file);
                }
                else
                {
                    _logger.Log(LogLevel.Error, $"File of metadata with id='{id}' not found in file storage");
                    return NotFound($"File of metadata with id='{id}' not found in file storage");
                }
                _logger.Log(LogLevel.Trace, $"File of metadata with id='{id}' successfully fetched.");
            }
            
            _logger.Log(LogLevel.Debug, "Get metadata method is over.");
            return Ok(files);
        }
    }
}
