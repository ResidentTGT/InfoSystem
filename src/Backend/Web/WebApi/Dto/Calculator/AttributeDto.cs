using System;
using System.Collections.Generic;

namespace WebApi.Dto.Calculator
{
    public class AttributeDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int? GroupId { get; set; }

        /* List Type */
        public int? ListId { get; set; }

        /* Str or Text Type */
        public string Template { get; set; }

        public int? MaxLength { get; set; }

        public int? MinLength { get; set; }

        /* Num Type */
        public int? Max { get; set; }

        public int? Min { get; set; }

        /* Date Type */
        public DateTime? MinDate { get; set; }

        public DateTime? MaxDate { get; set; }

        public List<int> CategoriesIds { get; set; } = new List<int>();

        public AttributeDto()
        {

        }
    }
}
