using System;
using System.Collections.Generic;
using System.Text;

namespace Company.Common.Requests.Pim
{
    public class ProductCmpySkusAndAttributesIdsRequest
    {
        public List<string> Skus { get; set; }
        public List<int> AttributesIds { get; set; } = new List<int>();
    }
}
