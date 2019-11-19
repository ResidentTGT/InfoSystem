using Microsoft.AspNetCore.Http;
using Company.Common.Models.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Pim.Client.v2
{
    public interface IFileStorageMsCommunicator
    {
        File SaveFile(HttpContext context);
        File SaveImage(byte[] file, string fileName);
    }
}
