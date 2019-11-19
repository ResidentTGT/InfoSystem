using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class ImportResponseDto
    {
        public Import Import { get; set; }
        public string ManagerName { get; set; }
    }
}