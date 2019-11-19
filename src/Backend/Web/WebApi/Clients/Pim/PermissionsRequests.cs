using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Company.Common;
using Company.Common.Models.Users;
using Company.Common.Factories;

namespace WebApi.Clients.Pim
{
    public class PermissionsRequests
    {
        private readonly IClientProviderFactory<HttpClient> _clientFactory;
        private readonly string _baseUri;

        public PermissionsRequests(IClientProviderFactory<HttpClient> clientFactory, string baseUri)
        {
            _clientFactory = clientFactory;
            _baseUri = baseUri;
        }

        public async Task<HttpResponseMessage> GetRoles()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/permissions/roles");
        }


        public async Task<HttpResponseMessage> GetUserRoles()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/permissions/user-roles");
        }

        public async Task<HttpResponseMessage> GetResourcesNames()
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/permissions/resources/names");
        }

        public async Task<HttpResponseMessage> GetResourcesPermissionCmpyRole(int roleId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/permissions/resources/{roleId}");
        }

        public async Task<HttpResponseMessage> GetAttributesPermissionCmpyUserId(int userId)
        {
            return await _clientFactory.GetClientWithHeaders().GetAsync($"{_baseUri}/permissions/attributes/{userId}");
        }

        public async Task<HttpResponseMessage> CreateSectionPermission(SectionPermission sectionPermission)
        {
            var content = new StringContent(JsonConvert.SerializeObject(sectionPermission), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/permissions/sections", content);
        }

        public async Task<HttpResponseMessage> CreateResourcePermission(ResourcePermission resourcePermission)
        {
            var content = new StringContent(JsonConvert.SerializeObject(resourcePermission), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/permissions/resources", content);
        }


        public async Task<HttpResponseMessage> EditResourcePermission(int id, ResourcePermission resourcePermission)
        {
            var content = new StringContent(JsonConvert.SerializeObject(resourcePermission), Encoding.UTF8, "application/json");
            return await _clientFactory.GetClientWithHeaders().PostAsync($"{_baseUri}/permissions/resources/{id}", content);
        }


        public async Task<HttpResponseMessage> DeleteSectionPermission(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/permissions/sections/{id}");
        }

        public async Task<HttpResponseMessage> DeleteResourcePermission(int id)
        {
            return await _clientFactory.GetClientWithHeaders().DeleteAsync($"{_baseUri}/permissions/resources/{id}");
        }
    }
}