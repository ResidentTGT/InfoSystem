using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NLog;


namespace WebApi.Extensions
{
    public static class Common
    {
        public static async Task<object> ResponseToDto<TResponse>(this HttpResponseMessage response,
            Func<TResponse, object> convertToDto = null,
            HttpStatusCode successCode = HttpStatusCode.OK)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Starting processing response to Dto method...");
            logger.Trace("Reading response...");
            var json = await response.Content.ReadAsStringAsync();
            object result = json;
            logger.Trace("Response successfully read");
            if (response.StatusCode == successCode)
            {
                logger.Trace($"Converting response to entity of type {typeof(TResponse)}...");
                var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(json);
                logger.Trace($"Response successfully converted");
                logger.Trace($"Converting entity to Dto...");
                result = convertToDto == null ? entity : convertToDto(entity);
                logger.Trace($"Entity successfully converted to Dto");
            }
            logger.Info("Response to Dto method is finished");
            return result;
        }

        public static async Task<IActionResult> ResponseToFile(this HttpResponseMessage response, IHttpContextAccessor contextAccessor,
            HttpStatusCode successCode = HttpStatusCode.OK)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Starting processing response to file method...");
           
            if (response.StatusCode == successCode)
            {
                logger.Trace("Reading file...");
                var file = await response.Content.ReadAsStreamAsync();
                logger.Trace("File successfully read");
                logger.Info("Response to file method is finished");
                return new FileStreamResult(file, response.Content.Headers.ContentType.MediaType)
                {
                    FileDownloadName = response.Content.Headers.ContentDisposition.FileNameStar
                };
            }
            logger.Warn("Response has non-successful status code");
            logger.Trace("Setting status code to response message...");
            contextAccessor.HttpContext.Response.StatusCode = (int)response.StatusCode;
            logger.Info("Response to file method is finished");
            return new ObjectResult(await response.Content.ReadAsStringAsync());
        }

        public static List<int> GetIdsFromQuery(this HttpRequest request, string paramName = "ids", string separator = ",")
        {
            var ids = new List<int>();
            if (request.Query.ContainsKey(paramName) && !String.IsNullOrEmpty(request.Query[paramName]))
            {
                ids = request.Query[paramName].ToString().Split(',').Select(int.Parse).ToList();
            }
            return ids;
        }

        public static List<string> GetSkuFromQuery(this HttpRequest request, string paramName = "sku", string separator = ",")
        {
            var ids = new List<string>();
            if (request.Query.ContainsKey(paramName) && !String.IsNullOrEmpty(request.Query[paramName]))
            {
                ids = request.Query[paramName].ToString().Split(',').ToList();
            }
            return ids;
        }


        public static MultipartFormDataContent FileToMultipartFormDataContent(this IFormFile file)
        {
            if (file == null)
                return null;

            var multiContent = new MultipartFormDataContent
            {
                {
                    new StreamContent(file.OpenReadStream())
                    {
                        Headers =
                        {
                            ContentLength = file.Length,
                            ContentType = new MediaTypeHeaderValue(file.ContentType)
                        }
                    },
                    "file", file.FileName
                }
            };
            return multiContent;
        }
    }
}