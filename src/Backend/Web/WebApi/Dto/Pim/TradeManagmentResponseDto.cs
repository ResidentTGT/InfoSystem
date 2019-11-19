using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class TradeManagmentResponseDto
    {
        public Product Product { get; set; }
        public List<Category> Categories { get; set; }
    }
}