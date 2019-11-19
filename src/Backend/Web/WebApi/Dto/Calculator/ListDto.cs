using System.Collections.Generic;

namespace WebApi.Dto.Calculator
{
    public class ListDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<ListValueDto> ListValues { get; set; } = new List<ListValueDto>();

        public ListDto()
        {

        }

      
    }
}
