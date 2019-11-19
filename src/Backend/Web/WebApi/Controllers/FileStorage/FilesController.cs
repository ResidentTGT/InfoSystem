using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Company.Common.Models.Identity;
using Company.Common.Models.Seasons;
using WebApi.Attributes;
using WebApi.Clients;
using WebApi.Extensions;
using File = Company.Common.Models.FileStorage.File;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApi.Controllers.FileStorage
{
    [Produces("application/json")]
    [Route("v1/[controller]")]
    public class FilesController : Controller
    {
        private IHttpFileStorageClient _httpFileStorageClient;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly Logger _logger;

        public FilesController(IHttpFileStorageClient httpFileStorageClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpFileStorageClient = httpFileStorageClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = LogManager.GetCurrentClassLogger();
        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            _logger.Debug($"Start getting file by id={id} method...");
            _logger.Trace($"Getting  file by id={id} from MS FileStorage...");
            var response = await _httpFileStorageClient.FileStorage.GetById(id);
            _logger.Trace($"File by id={id} from MS FileStorage successfully received");
            _logger.Trace($"Converting response stream to file...");
            var responseFile = await response.ResponseToFile(_httpContextAccessor);
            _logger.Trace($"Response stream successfully converted to file");
            _logger.Debug($"Getting  file by id={id} method is finished");
            return responseFile;

        }

        [HttpGet("metadata")]
        [ProducesResponseType(typeof(List<File>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetMetadata()
        {
            var ids = Request.GetIdsFromQuery();
            _logger.Debug($"Start getting metadata by ids={ids} method...");
            _logger.Trace($"Getting metadata by ids={ids} from MS FileStorage...");
            var response = await _httpFileStorageClient.FileStorage.GetMetadata(ids);
            _logger.Trace($"Logistic by metadata by ids={ids} from MS FileStorage successfully received");
            _logger.Trace($"Converting response to List of File...");
            var responseObject = await response.ResponseToDto<List<File>>();
            _logger.Trace($"Response successfully converted to List of File");
            _logger.Debug($"Getting metadata by ids={ids} method is finished");
            return StatusCode((int)response.StatusCode, responseObject);
        }


        [HttpPost]
        [ProducesResponseType(typeof(File), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> Save()
        {
            _logger.Debug($"Start saving file method...");

            _logger.Debug($"Getting file from request...");
            var file = _httpContextAccessor.HttpContext.Request.Form.Files.FirstOrDefault().FileToMultipartFormDataContent();
            if (file == null)
            {
                _logger.Error($"File not found in request");
                return NotFound();
            }

            _logger.Debug($"File successfully got");
            using (file)
            {
                _logger.Trace($"Saving file to MS FileStorage...");
                var response = await _httpFileStorageClient.FileStorage.Save(file);
                _logger.Trace($"Saving file response from MS FileStorage successfully received");
                _logger.Trace($"Converting response to File...");
                var responseObject = await response.ResponseToDto<File>();
                _logger.Trace($"Response successfully converted to File");
                _logger.Debug($"Saving file method is finished");
                return StatusCode((int)response.StatusCode, responseObject);
            }
        }
    }
}