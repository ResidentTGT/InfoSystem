using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Company.Common.Enums;
using Company.Common.Models.Pim;

namespace WebApi.Dto.Pim
{
    public class AttributeCategoryDto
    {
        public int CategoryId { get; set; }

        public int AttributeId { get; set; }

        public ModelLevel ModelLevel { get; set; }

        public bool IsRequired { get; set; }

        public bool IsFiltered { get; set; }

        public bool IsVisibleInProductCard { get; set; }

        public bool IsKey { get; set; }

        public AttributeCategory ToEntity()
        {
            return new AttributeCategory()
            {
                CategoryId = CategoryId,
                AttributeId = AttributeId,
                ModelLevel = ModelLevel,
                IsRequired = IsRequired,
                IsFiltered = IsFiltered,
                IsVisibleInProductCard = IsVisibleInProductCard,
                IsKey = IsKey
            };
        }
    }
}