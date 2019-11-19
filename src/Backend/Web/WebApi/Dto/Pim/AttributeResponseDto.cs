using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Models.Pim;
using Attribute = Company.Common.Models.Pim.Attribute;

namespace WebApi.Dto.Pim
{
    public class AttributeResponseDto
    {
        public Attribute Attribute { get; set; }
        public bool WithCategories { get; set; }
    }
}