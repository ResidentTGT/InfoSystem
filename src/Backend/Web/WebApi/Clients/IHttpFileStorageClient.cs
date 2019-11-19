using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebApi.Clients.FileStorage;

namespace WebApi.Clients
{
    public interface IHttpFileStorageClient
    {
        FileStorageRequests FileStorage { get; }
    }
}