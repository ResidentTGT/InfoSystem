using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Web1cApi.Extensions
{
    public static class Common
    {
        public static async Task<TResult> HttpResponseToDto<TResponse, TResult>(this HttpResponseMessage response,
            Func<TResponse, TResult> convertToDto)
        {
            var entity = Newtonsoft.Json.JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            return convertToDto(entity);
        }

        public static async Task<IActionResult> HttpResponseToFile(this HttpResponseMessage response, IHttpContextAccessor contextAccessor,
            HttpStatusCode successCode = HttpStatusCode.OK)
        {
            if (response.StatusCode == successCode)
            {
                var file = await response.Content.ReadAsStreamAsync();
                return new FileStreamResult(file, response.Content.Headers.ContentType.MediaType)
                {
                    FileDownloadName = response.Content.Headers.ContentDisposition.FileNameStar
                };
            }
            contextAccessor.HttpContext.Response.StatusCode = (int) response.StatusCode;
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