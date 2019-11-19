using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Company.Common.Extensions
{
    public static class Common
    {
        public static List<string> GetArrayFromQuery(this HttpRequest request, string paramName, string separator = ",")
        {
            var array = new List<string>();
            if (request.Query.ContainsKey(paramName) && !string.IsNullOrEmpty(request.Query[paramName]))
            {
                array = request.Query[paramName].ToString().Split(separator).ToList();
            }
            return array;
        }

        public static List<int> GetIdsFromQuery(this HttpRequest request, string paramName = "ids", string separator = ",")
        {
            return request.GetArrayFromQuery(paramName, separator).Select(int.Parse).ToList();
        }
    }
}
