using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;

namespace Web1cApi.Dto
{
    public class TradeManagementResponseDto
    {
        public Product Product { get; set; }
        public string CategoryTreeString { get; set; }
    }
}