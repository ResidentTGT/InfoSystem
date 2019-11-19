using System.Collections.Generic;

namespace Company.Deals.Structures
{
    public class OrderFormParseResult
    {
        public OrderFormParseResult()
        {
           
        }

        public Dictionary<string, int> skusAndCountsDictionary;

        public string Brand { get; set; }
        public int BrandId { get; set; }
        public string Tin { get; set; }
        public string Rrc { get; set; }
       
    }
}
