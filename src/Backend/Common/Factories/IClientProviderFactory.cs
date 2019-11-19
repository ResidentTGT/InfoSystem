using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Company.Common.Factories
{
    public interface IClientProviderFactory<T>
    {
        T GetClientWithHeaders();
    }
}
