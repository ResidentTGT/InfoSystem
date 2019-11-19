using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using File = Company.Common.Models.FileStorage.File;
using NLog;

namespace Company.Deals.Client.v2
{
    public class HttpFileStorageMsCommunicator
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Logger _logger;

        public HttpFileStorageMsCommunicator()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public File SaveDealFile(HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Starting the save deal file method...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers..."); 
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
            _logger.Log(LogLevel.Trace, "User data to headers are added");

            _logger.Log(LogLevel.Trace, "Reading data from file stream...");
            byte[] data;
            using (var binaryRead = new BinaryReader(context.Request.Form.Files.FirstOrDefault().OpenReadStream()))
            {
                data = binaryRead.ReadBytes((int)context.Request.Form.Files.FirstOrDefault().OpenReadStream().Length);
            }
            _logger.Log(LogLevel.Trace, "Reading data from file stream is finished");
            var bytes = new ByteArrayContent(data);
            _logger.Log(LogLevel.Trace, "Adding data to multipart form...");
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(bytes, "file", context.Request.Form.Files.FirstOrDefault().FileName);
            _logger.Log(LogLevel.Trace, "Data added to multipart form");

            _logger.Log(LogLevel.Debug, "Save deal file method is over.");
            return JsonConvert.DeserializeObject<File>(client.PostAsync("/v2/files",
                form).Result.Content.ReadAsStringAsync().Result);
        }

        public async Task<FileStreamResult> DownloadFile(int id)
        {
            _logger.Log(LogLevel.Debug, "Starting the download file method...");

            _logger.Log(LogLevel.Trace, "Getting response...");
            var response = await client.GetAsync($"/v2/files/{id}");
            _logger.Log(LogLevel.Trace, "Response successfully fetched");

            _logger.Log(LogLevel.Trace, "Creating file stream result...");
            FileStreamResult resultStream = new FileStreamResult(await response.Content.ReadAsStreamAsync(), "application/octet-stream");
            resultStream.FileDownloadName = response.Content.Headers.ContentDisposition.FileNameStar;
            _logger.Log(LogLevel.Trace, $"File stream '{resultStream.FileDownloadName}' result is created");

            _logger.Log(LogLevel.Debug, "Download file method is over.");
            return resultStream;
        }

    }
}
