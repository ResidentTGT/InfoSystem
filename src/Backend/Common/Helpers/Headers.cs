using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;

namespace Company.Common.Helpers
{
    public static class Headers
    {
        public static string GetHeaderValue(IHeaderDictionary headers, string key) =>
            headers.ContainsKey(key) && headers.TryGetValue(key, out StringValues userIdStr)
            ? userIdStr.ToString()
            : null;

        public static void SetHeader(HttpRequestHeaders headers, string key, string value)
        {
            if (headers.Contains(key))
                headers.Remove(key);

            headers.Add(key, value);
        }
    }
}
