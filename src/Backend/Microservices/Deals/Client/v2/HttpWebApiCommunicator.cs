using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Deals.Client.v2
{
    public class HttpWebApiCommunicator
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<HttpResponseMessage> GetUser(int id)
            => await client.GetAsync($"/api/v1/users/info?userId={id}");

        public async Task<HttpResponseMessage> GetUsersFromDepartmentTree(int departmentId)
            => await client.GetAsync($"/api/v1/users?departmentId={departmentId}");

        public async Task<HttpResponseMessage> GetUserCmpyDepartmentsIds(List<int> departmentIds)
        {
            var ids = string.Join(",", departmentIds);
            return await client.GetAsync($"/api/v1/users/byDepartmentsIds?departmentsIds={ids}");
        }
    }
}
