using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Net.Http;
using File = Company.Common.Models.FileStorage.File;
using NLog;

namespace Company.Pim.Client.v2
{
    public class HttpFileStorageMsCommunicator : IFileStorageMsCommunicator
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Logger _logger;

        public HttpFileStorageMsCommunicator()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public File SaveFile(HttpContext context)
        {
            _logger.Log(LogLevel.Debug, "Start saving file...");

            _logger.Log(LogLevel.Trace, "Adding user data to headers...");
            var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "UserId", userId);
            Common.Helpers.Headers.SetHeader(client.DefaultRequestHeaders, "ClientIp", context.Connection.RemoteIpAddress.ToString());
            _logger.Log(LogLevel.Trace, "User data is added to headers.");

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

            _logger.Log(LogLevel.Trace, "Sending file to file storage MS...");
            var res = JsonConvert.DeserializeObject<File>(client.PostAsync("/v2/files",
                form).Result.Content.ReadAsStringAsync().Result);
            _logger.Log(LogLevel.Debug, "Saving file is finished.");

            return res;
        }

        public File SaveImage(byte[] file, string fileName)
        {
            //var userId = Common.Helpers.Headers.GetHeaderValue(context.Request.Headers, "UserId");
            //client.DefaultRequestHeaders.Add("UserId", userId);
            //client.DefaultRequestHeaders.Add("ClientIp", context.Connection.RemoteIpAddress.ToString());
            _logger.Log(LogLevel.Debug, $"Start saving image '{fileName}' ...");

            var bytes = new ByteArrayContent(file);
            _logger.Log(LogLevel.Trace, "Adding data to multipart form...");
            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(bytes, "file", fileName);
            _logger.Log(LogLevel.Trace, "Data added to multipart form.");

            _logger.Log(LogLevel.Trace, "Sending image to file storage MS...");
            var res = JsonConvert.DeserializeObject<File>(client.PostAsync("/v2/files",
                form).Result.Content.ReadAsStringAsync().Result);
            _logger.Log(LogLevel.Debug, "Saving image is finished.");
            return res;
        }
    }
}
