using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Company.Pim.Client.v2
{
    public interface IWebApiCommunicator
    {
        Task<HttpResponseMessage> GetUser(int id);
    }
}
